using CSGORUN_Robot.Client.GameControllers;
using CSGORUN_Robot.CSGORUN.DTOs;
using CSGORUN_Robot.CSGORUN.Exceptions;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Extensions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Client
{
    public class ClientWorker
    {
        public Account Account { get; set; }

        private readonly BlockingCollection<string> _promoQueue = new();
        private Task _promoProcessThread;
        private readonly ILogger _log = Log.Logger.ForContext<ClientWorker>();
        private static readonly Random _random = new Random();

        private readonly TimeSpan _promoCacheLifetime = TimeSpan.FromMinutes(AppSettingsProvider.Provide().CSGORUN.PromoCache.Lifetime_Minutes);
        private readonly AppSettings _settings = AppSettingsProvider.Provide();
        private readonly Dictionary<string, DateTime> _promoCache = new();
        private readonly IGameController _defaultGameInteraction;

        public ClientHttpService HttpService { get; set; }

        public ClientWorker(Account account)
        {
            Account = account;
            HttpService = new ClientHttpService(account.AuthToken, account.UserAgent, account.Proxy?.ToWebProxy());
            account.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(account.AuthToken))
                    HttpService.UpdateToken(account.AuthToken);
                else if (e.PropertyName == nameof(account.UserAgent))
                    HttpService.UpdateUserAgent(account.UserAgent);
            };

            _defaultGameInteraction = new RouletteGameController(this);

            StartPromoProcessThread();
        }

        public void EnqueuePromo(string promo)
        {
            if (HttpService.IsAuthorized)
                _promoQueue.Add(promo.Trim());
        }


        public void StartPromoProcessThread()
        {
            while (_promoQueue.TryTake(out _)) { }
            _promoProcessThread = Task.Run(PromoProcessThread);
        }

        private void CacheCleanup()
        {
            var activeFrom = DateTime.Now - _promoCacheLifetime;
            var toDelete = _promoCache.Where(t => t.Value <= activeFrom).Select(t => t.Key);
            foreach (var key in toDelete)
            {
                _promoCache.Remove(key);   
            }
        }

        public async void PromoProcessThread()
        {
            foreach (var promo in _promoQueue.GetConsumingEnumerable())
            {
                CacheCleanup();
                if (_promoCache.ContainsKey(promo.ToLower()))
                {
                    _log.Information("[{0}] {1} - '{2}': Promo was already checked.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo);
                    continue;
                }

                var receivedTime = DateTime.Now;

                GetCurrentState:
                try
                {
                    await HttpService.GetCurrentStateAsync();
                }
                catch (HttpRequestRawException ex)
                {
                    var inner = ex.InnerException;
                    if (inner.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _log.Fatal("[{0}] {1}'s CurrentState request returns unauthorized!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        _promoQueue.Clear();
                        continue;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _log.Warning("[{0}] {1} has too many request!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        await Task.Delay(2000);
                        goto GetCurrentState;
                    }
                    
                    _log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }

                // Random Delay
                TimeSpan currentDelay = DateTime.Now - receivedTime;
                int randDelay = _random.FromRange(_settings.CSGORUN.BeforeActivationDelay);

                int currentMs = (int)currentDelay.TotalMilliseconds;
                if (currentMs < randDelay)
                {
                    int delay = randDelay - currentMs;
                    _log.Information("[{0}] {1} awaiting delay {2}ms.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, delay);
                    await Task.Delay(delay);
                }

                ActivatePromo:
                try
                {
                    var balance = await HttpService.PostActivatePromoAsync(promo);
                    if (_settings.CSGORUN.AutoPlaceBet)
                    {
                        var skipGames = _random.FromRange(_settings.CSGORUN.PlaceBetSkipGames);
                        await PerformDefaultBetAsync(skipGames);
                    }

                    var bot = Program.ServiceProvider.GetRequiredService<TelegramBotService>();
                    _log.Warning("[{0}] {1}: Promo '{2}' was activated! (+{3}$). Balance: {4}$", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo, balance.added ?? 0, HttpService.LastCurrentState.user.balance);
                    await bot.SendMessageToOwnerAsync(string.Format("[{0}] {1}: Promo '{2}' was activated! (+{3}$). Balance: {4}$", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo, balance.added ?? 0, HttpService.LastCurrentState.user.balance));

                    await Task.Delay(_settings.CSGORUN.RequestsDelay);
                }
                catch (HttpRequestRawException ex)
                {
                    var inner = ex.InnerException;
                    var content = await ex.Content.ReadAsStringAsync();
                    if (inner.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _log.Fatal("[{0}] {1}'s CurrentState request returns unauthorized!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        break;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _log.Warning("[{0}] {1} has too many request!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        await Task.Delay(2000);
                        goto ActivatePromo;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        _log.Warning("[{0}] {1}'s: Promo '{2}' - Cannot be used. Details: {3}", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo, content);

                        var d = JsonSerializer.Deserialize<ErrorResponse>(content);
                        if (d.error == "TESAK_DIDNT_KILL_HIMSELF" && _settings.CSGORUN.AutoPlaceBet)
                        {
                            await PerformDefaultBetAsync();
                        }
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _log.Warning("[{0}] {1}'s: Promo '{2}' - Not found!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo);
                        await Task.Delay(_settings.CSGORUN.RequestsDelay);
                    }
                    else
                    {
                        _log.Error("[{0}] {1}'s: Promo '{2}' - Exception. Details: {3}", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo, content);
                        await Task.Delay(_settings.CSGORUN.RequestsDelay);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "[{0}] {1}'s: Promo '{2}' - Exception.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo);
                    continue;
                }

                // Promo encache
                _promoCache.Add(promo.ToLower(), DateTime.Now);
            }
        }

        public async Task<int> ProvideItemAsync(double price = 0.25, bool refreshCurrentState = false)
        {
            if (refreshCurrentState) await HttpService.GetCurrentStateAsync();
            var item = HttpService.LastCurrentState.user.items.FirstOrDefault(t => t.price == price)?.id;

            if (item == null)
            {
                if (HttpService.LastCurrentState.user.balance < price)
                    throw new BalanceLessTargetException($"You wanna buy an item with price {price}, but your balance is {HttpService.LastCurrentState.user.balance}");

                var items = await HttpService.GetMarketItemsAsync();
                var itemIdObj = items?.FirstOrDefault(t => ((JsonElement)t[6]).GetDouble() == price)?[0];

                if (itemIdObj == null)
                    throw new MarketItemNotFoundException($"Item with described parameters not found on market");

                var itemId = ((JsonElement)itemIdObj).GetInt32();

                item = await HttpService.PostBuyItemAsync(itemId);
            }

            return item.Value;
        }

        public async Task PerformDefaultBetAsync(int skipGames = 0, IGameController game = null)
        {
            game ??= _defaultGameInteraction;

            Retry:
            _log.Information("[{0}] {1}: Placing bet after {2} games...", nameof(PerformDefaultBetAsync), HttpService.LastCurrentState.user.name, skipGames);
            try
            {
                var itemId = await ProvideItemAsync(0.25);
                for (int i = 0; i < skipGames + 1; i++)
                    await game.AwaitGameStartAsync();

                // Random Delay
                int randDelay = _random.FromRange(_settings.CSGORUN.PlaceBetDelayAfterGameStartDelay);
                _log.Information("[{0}] {1}: Game starting... Awaiting delay {2}ms.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, randDelay);
                await Task.Delay(randDelay);

                await game.MakeQuickBetAsync(itemId);
                _log.Information("[{0}] {1}: Bet placed success!", nameof(PerformDefaultBetAsync), HttpService.LastCurrentState.user.name);
            }
            catch (HttpRequestRawException ex)
            {
                if (ex.InnerException.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    goto Retry;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{0}] {1}:", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
            }
        }

        public async Task AwaitCrashGameStartAsync()
        {
            var worker = Program.ServiceProvider.GetRequiredService<Worker>();
            var csgorun = worker.Aggregators.OfType<CsgorunService>().First();
            var resetEvent = new ManualResetEvent(false);
            EventHandler awaiter = (s, e) => resetEvent.Set();
            csgorun.CrashGameStarted += awaiter;
            await Task.Run(() => resetEvent.WaitOne());
            csgorun.CrashGameStarted -= awaiter;
        }

        public async Task AwaitRouletteGameStartAsync()
        {
            var worker = Program.ServiceProvider.GetRequiredService<Worker>();
            var csgorun = worker.Aggregators.OfType<CsgorunService>().First();
            var resetEvent = new ManualResetEvent(false);
            EventHandler awaiter = (s, e) => resetEvent.Set();
            csgorun.RouletteGameStarted += awaiter;
            await Task.Run(() => resetEvent.WaitOne());
            csgorun.RouletteGameStarted -= awaiter;
        }

        public async Task<List<WithdrawItem>> GetWithdrawsAsync()
        {
            var delay = _settings.CSGORUN.RequestsDelay;

            var items = new List<WithdrawItem>();

            var pages = 1;
            for (int page = 1; page <= pages; page++)
            {
                if (page != 1) await Task.Delay(delay);

                var r = await HttpService.GetWithdrawsAsync(page);
                pages = r.pages;

                items.AddRange(r.items);
            }

            return items;
        }
    }
}

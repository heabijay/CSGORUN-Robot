using CSGORUN_Robot.CSGORUN.DTOs;
using CSGORUN_Robot.CSGORUN.Exceptions;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Client
{
    public class ClientWorker
    {
        public Account Account { get; set; }

        private readonly BlockingCollection<string> promoQueue = new();
        private Task promoProcessThread;
        private readonly ILogger log = Log.Logger.ForContext<ClientWorker>();
        private static Random random = new Random();

        public ClientHttpService HttpService { get; set; }

        public ClientWorker(Account account)
        {


            Account = account;
            HttpService = new ClientHttpService(account.AuthToken, account.Proxy?.ToWebProxy());
            account.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(account.AuthToken))
                    HttpService.UpdateToken(account.AuthToken);
            };

            StartPromoProcessThread();
        }

        public void EnqueuePromo(string promo)
        {
            promoQueue.Add(promo);
        }


        public void StartPromoProcessThread()
        {
            while (promoQueue.TryTake(out _)) { }
            promoProcessThread = Task.Run(PromoProcessThread);
        }

        public async void PromoProcessThread()
        {
            foreach (var promo in promoQueue.GetConsumingEnumerable())
            {
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
                        log.Fatal("[{0}] {1}'s CurrentState request returns unauthorized!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        break;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        log.Warning("[{0}] {1} has too many request!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        await Task.Delay(2000);
                        goto GetCurrentState;
                    }
                    
                    log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }
                catch (Exception ex)
                {
                    log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }

                // Random Delay
                TimeSpan currentDelay = DateTime.Now - receivedTime;
                int randDelay = random.Next(1000, 2500);

                int currentMs = (int)currentDelay.TotalMilliseconds;
                if (currentMs < randDelay)
                {
                    int delay = randDelay - currentMs;
                    log.Information("[{0}] {1} awaiting delay {2}ms.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, delay);
                    await Task.Delay(delay);
                }

                ActivatePromo:
                try
                {
                    await HttpService.PostActivatePromoAsync(promo);
                }
                catch (HttpRequestRawException ex)
                {
                    var inner = ex.InnerException;
                    if (inner.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        log.Fatal("[{0}] {1}'s CurrentState request returns unauthorized!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        break;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        log.Warning("[{0}] {1} has too many request!", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                        await Task.Delay(2000);
                        goto ActivatePromo;
                    }
                    else if (inner.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var content = await ex.Content.ReadAsStringAsync();
                        log.Warning("[{0}] {1}'s: Promo '{2}' - Cannot be used. Details: {3}", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name, promo, content);

                        var d = JsonSerializer.Deserialize<ErrorResponse>(content);
                        if (d.error == "TESAK_DIDNT_KILL_HIMSELF")
                        {

                        }
                    }
                    //else if ()

                    
                }
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
                var itemIdObj = items?.FirstOrDefault(t => Convert.ToDouble(t[6]) == price)?[0];

                if (itemIdObj == null)
                    throw new MarketItemNotFoundException($"Item with described parameters not found on market");

                item = Convert.ToInt32(itemIdObj);
            }

            return item.Value;
        }


        public async Task<List<WithdrawItem>> GetWithdrawsAsync()
        {
            var delay = (await AppSettingsProvider.ProvideAsync()).CSGORUN.RequestsDelay;

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

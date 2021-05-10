using CSGORUN_Robot.CSGORUN.Exceptions;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Client
{
    public class ClientWorker
    {
        public Account Account { get; set; }

        private readonly BlockingCollection<string> promoQueue = new();
        private Task promoProcessThread;
        private readonly ILogger log = Log.Logger.ForContext<ClientWorker>();

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
                        EnqueuePromo(promo);
                    }
                    
                    log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }
                catch (Exception ex)
                {
                    log.Error(ex, "[{0}] {1} has error while proceeding CurrentState.", nameof(PromoProcessThread), HttpService.LastCurrentState.user.name);
                    continue;
                }

                try
                {
                    await HttpService.PostActivatePromoAsync(promo);
                }
                catch (HttpRequestRawException ex)
                {
                    var content = await ex.Content.ReadAsStringAsync();
                    var inner = ex.InnerException;
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
    }
}

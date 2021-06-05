using CSGORUN_Robot.CSGORUN.DTOs;
using CSGORUN_Robot.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Services
{
    public class ClientHttpService
    {
        protected HttpClient httpClient { get; set; } = new HttpClient().SetCSGORUNHttpHeaders();

        public ClientHttpService(string authToken, IWebProxy proxy = null)
        {
            if (proxy != null)
                UpdateProxy(proxy);

            UpdateToken(authToken);
        }

        public CurrentState LastCurrentState { get; set; }

        public bool IsAuthorized { get; private set; } = true;

        public void UpdateProxy(IWebProxy proxy)
        {
            var old = httpClient;
            httpClient = new HttpClient(
                new HttpClientHandler()
                {
                    Proxy = proxy,
                }).SetCSGORUNHttpHeaders();

            UpdateToken(old?.DefaultRequestHeaders?.Authorization?.Parameter);

            old?.Dispose();
        }

        public void UpdateToken(string authToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("JWT", authToken);
        }

        protected async Task InvokeRequestAsync(HttpRequestMessage req) => await InvokeRequestAsync<object>(req);
        protected async Task<T> InvokeRequestAsync<T>(HttpRequestMessage req) where T : new()
        {
            var result = await httpClient.SendAsync(req);

            // Unauthorized handle
            if (result.StatusCode == HttpStatusCode.Unauthorized) IsAuthorized = false;
            else if (result.IsSuccessStatusCode) IsAuthorized = true;

            result.EnsureSuccessStatusCodeRaw();
            var content = await result.Content.ReadAsStreamAsync();
            var resp = await JsonSerializer.DeserializeAsync<SuccessResponse<T>>(content);
            return resp.data;
        }


        public async Task<CurrentState> GetCurrentStateAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(CSGORUN.Routing.CurrentState))
            {

            };

            var resp = await InvokeRequestAsync<CurrentState>(req);
            LastCurrentState = resp;
            return resp;
        }

        public async Task PostActivatePromoAsync(string promo)
        {
            var data = new Promo() { code = promo };

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(CSGORUN.Routing.Promo))
            {
                Headers =
                {
                    { HttpRequestHeader.Referer.ToString(), CSGORUN.Routing.HomeProfileEndpoint + "/" + LastCurrentState?.user?.steamId },
                },
                Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json;charset=UTF-8")
            };

            var balance = await InvokeRequestAsync<BalanceUpdate>(req);
            LastCurrentState.user.balance = balance.balance + balance.added ?? 0;
        }

        public async Task<List<List<object>>> GetMarketItemsAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(CSGORUN.Routing.MarketItems))
            {

            };

            return await InvokeRequestAsync<List<List<object>>>(req);
        }

        public async Task<int> PostBuyItemAsync(int itemId)
        {
            var data = new ItemsNeedExchange()
            {
                userItemIds = new(),
                wishItemIds = new(itemId)
            };

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(CSGORUN.Routing.ExchangeItems))
            {
                Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json;charset=UTF-8")
            };

            var resp = await InvokeRequestAsync<ItemsExchange>(req);
            LastCurrentState.user.balance = resp.balance;
            return resp.userItems.newItems[0].id;
        }

        public async Task PostMakeBetAsync(int itemId, double autoCoefficient)
        {
            var data = new PlaceBet()
            {
                userItemIds = new(itemId),
                auto = autoCoefficient
            };

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(CSGORUN.Routing.MakeBet))
            {
                Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json;charset=UTF-8")
            };

            await InvokeRequestAsync(req);
        }

        public async Task<PaginationResult<WithdrawItem>> GetWithdrawsAsync(int page = 1)
        {
            var userId = LastCurrentState?.user?.id ?? (await GetCurrentStateAsync()).user.id;

            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(CSGORUN.Routing.Withdraws(userId, page)))
            {
                
            };

            return await InvokeRequestAsync<PaginationResult<WithdrawItem>>(req);
        }
    }
}

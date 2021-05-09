using CSGORUN_Robot.CSGORUN.Dtos;
using CSGORUN_Robot.Extensions;
using CSGORUN_Robot.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        public void UpdateProxy(IWebProxy proxy)
        {
            var old = httpClient;
            httpClient = new HttpClient(
                new HttpClientHandler()
                {
                    Proxy = proxy,
                }).SetCSGORUNHttpHeaders();

            UpdateToken(old.DefaultRequestHeaders.Authorization.Parameter);

            old?.Dispose();
        }

        public void UpdateToken(string authToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("JWT", authToken);
        }


        public async Task<CurrentState> GetCurrentStateAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(CSGORUN.Routing.CurrentState))
            {
                Headers =
                {
                    { HttpRequestHeader.Referer.ToString(), CSGORUN.Routing.HomeEndpoint }
                },
            };

            var result = await httpClient.SendAsync(req);
            result.EnsureSuccessStatusCodeRaw();
            var content = await result.Content.ReadAsStreamAsync();
            var resp = await JsonSerializer.DeserializeAsync<SuccessResponse<CurrentState>>(content);
            LastCurrentState = resp.data;
            return resp.data;
        }

        public async Task PostActivatePromoAsync(string promo)
        {
            var data = new Promo() { code = promo };

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(CSGORUN.Routing.Promo))
            {
                Headers =
                {
                    { HttpRequestHeader.Referer.ToString(), CSGORUN.Routing.HomeEndpoint + "/profile/" + LastCurrentState?.user?.steamId },
                },
                Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json;charset=UTF-8")
            };

            var result = await httpClient.SendAsync(req);
            result.EnsureSuccessStatusCodeRaw();
            var content = await result.Content.ReadAsStreamAsync();
            var balance = await JsonSerializer.DeserializeAsync<BalanceUpdate>(content);
            LastCurrentState.user.balance = balance.balance + balance.added ?? 0;
        }
    }
}

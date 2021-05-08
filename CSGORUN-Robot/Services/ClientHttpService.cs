using CSGORUN_Robot.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Services
{
    public class ClientHttpService
    {
        protected HttpClient httpClient { get; set; } = new HttpClient();

        public ClientHttpService(string authToken, IWebProxy proxy = null)
        {
            if (proxy != null)
                UpdateProxy(proxy);

            UpdateToken(authToken);
        }

        public void UpdateProxy(IWebProxy proxy)
        {
            var old = httpClient;
            httpClient = new HttpClient(
                new HttpClientHandler()
                {
                    Proxy = proxy
                });

            old?.Dispose();
        }

        public void UpdateToken(string authToken)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("JWT", authToken);
        }
    }
}

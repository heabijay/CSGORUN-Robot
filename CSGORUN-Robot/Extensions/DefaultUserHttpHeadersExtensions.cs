using System.Net.Http;

namespace CSGORUN_Robot.Extensions
{
    public static class DefaultUserHttpHeadersExtensions
    {
        public static HttpClient SetCSGORUNHttpHeaders(this HttpClient client)
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("accept-language", "ru-RU,uk;q=0.9,ru;q=0.8,en-US;q=0.7,en;q=0.6");
            client.DefaultRequestHeaders.Add("origin", CSGORUN.Routing.HomeEndpoint);
            client.DefaultRequestHeaders.Add("referer", CSGORUN.Routing.HomeEndpoint);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");

            return client;
        }
    }
}

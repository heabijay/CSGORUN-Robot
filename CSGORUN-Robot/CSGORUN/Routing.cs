namespace CSGORUN_Robot.CSGORUN
{
    public static class Routing
    {
        public const string HomeEndpoint = "https://csgorun.gg";
        public const string ApiEndpoint = "https://api.csgorun.gg";

        public const string CurrentState = ApiEndpoint + "/current-state?montaznayaPena=null";
        public const string Promo = ApiEndpoint + "/discount";
        public const string CrashMakeBet = ApiEndpoint + "/make-bet";
        public const string RouletteMakeBet = ApiEndpoint + "/roulette/make-bet";
        public const string MarketItems = "https://cloud.this.team/csgo/items.json";
        public const string ExchangeItems = ApiEndpoint + "/marketplace/exchange-items";
        public const string WebSocket = "wss://ws.csgorun.gg/connection/websocket";
        public const string Profile = ApiEndpoint + "/profile";
        public const string HomeProfileEndpoint = HomeEndpoint + "/profile";

        public static string Withdraws(int userId, int page = 1) => $"{Profile}/{userId}/withdraws?page={page}";
    }
}

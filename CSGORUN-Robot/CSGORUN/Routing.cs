using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN
{
    public static class Routing
    {
        public const string HomeEndpoint = "https://csgorun.pro";
        public const string ApiEndpoint = "https://api.csgorun.pro";

        public const string CurrentState = ApiEndpoint + "/current-state?montaznayaPena=null";
        public const string Promo = ApiEndpoint + "/discount";
        public const string MakeBet = ApiEndpoint + "/make-bet";
        public const string MarketItems = "https://cloud.this.team/csgo/items.json";
        public const string ExchangeItems = ApiEndpoint + "/marketplace/exchange-items";
        public const string WebSocket = "wss://ws.csgorun.pro/connection/websocket";
    }
}

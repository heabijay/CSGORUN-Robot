using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class Subscription
    {
        public int method { get; set; }
        public SubscriptionParams @params { get; set; }
        public int id { get; set; }
    }
}

using System.Collections.Generic;

namespace CSGORUN_Robot.Settings
{
    public class TelegramAggregator
    {
        public int ApiId { get; set; }
        public string ApiHash { get; set; }
        public string PhoneNumber { get; set; }
        public List<TelegramChannels> Channels { get; set; }
    }
}

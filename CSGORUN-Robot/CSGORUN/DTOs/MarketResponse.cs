using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class MarketResponse
    {
        public bool status { get; set; }
        public int timestamp { get; set; }
        public string date { get; set; }
        public List<List<object>> data { get; set; }
    }
}

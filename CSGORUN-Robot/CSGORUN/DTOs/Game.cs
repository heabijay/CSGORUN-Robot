using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class Game
    {
        public double? delta { get; set; }
        public int? status { get; set; }
        public Statistic statistic { get; set; }
        public List<History> history { get; set; }
        public double? crash { get; set; }
        public object bet { get; set; }
        public string hash { get; set; }
        public List<Bet> bets { get; set; }
    }
}

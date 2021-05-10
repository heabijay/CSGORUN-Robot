using System.Collections.Generic;

namespace CSGORUN_Robot.Settings
{
    public class CSGORUN
    {
        public List<Account> Accounts { get; set; }

        public int PrimaryAccountIndex { get; set; }

        public RegexPatterns RegexPatterns { get; set; }

        public bool AutoPlaceBet { get; set; }
    }
}

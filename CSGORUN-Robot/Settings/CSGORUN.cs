using System.Collections.Generic;

namespace CSGORUN_Robot.Settings
{
    public class CSGORUN
    {
        public List<Account> Accounts { get; set; }

        public RegexPatterns RegexPatterns { get; set; }

        public bool AutoPlaceBet { get; set; }

        public int RequestsDelay { get; set; } = 5500;

        public PromoCache PromoCache { get; set; }

        public List<string> PromoExclusion { get; set; }
    }
}

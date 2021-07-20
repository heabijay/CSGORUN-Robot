using System.Collections.Generic;

namespace CSGORUN_Robot.Settings
{
    public class CSGORUN
    {
        public List<Account> Accounts { get; set; }

        public RegexPatterns RegexPatterns { get; set; }

        public bool AutoPlaceBet { get; set; }

        public int RequestsDelay { get; set; } = 5500;

        public Range BeforeActivationDelay { get; set; } = new()
        {
            Min = 1500,
            Max = 3000
        };

        public Range PlaceBetDelayAfterGameStartDelay { get; set; } = new()
        {
            Min = 1000,
            Max = 3000
        };

        public Range PlaceBetSkipGames { get; set; } = new()
        {
            Min = 0,
            Max = 0
        };

        public PromoCache PromoCache { get; set; }

        public List<string> PromoExclusion { get; set; }
    }
}

using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class RoulettePlaceBet : IPlaceBet
    {
        public List<int> userItemIds { get; set; }
        public RouletteColor number { get; set; }
    }
}

using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class CrashPlaceBet : IPlaceBet
    {
        public List<int> userItemIds { get; set; }
        public double auto { get; set; }
    }
}

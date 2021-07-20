using System;

namespace CSGORUN_Robot.Extensions
{
    public static class RandomRangeExtensions
    {
        public static int FromRange(this Random random, CSGORUN_Robot.Settings.Range range, bool isMaxInclusive = true)
        {
            return random.Next(range.Min, isMaxInclusive ? range.Max + 1 : range.Max);
        }
    }
}

using System;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public static class SubscriptionTypeExtensions
    {
        public static string ToStringEquivalent(this SubscriptionType type) => type.ToString().Replace('_', '-');

        public static SubscriptionType Parse(string str) => Enum.Parse<SubscriptionType>(str.Replace('-', '_'));
    }
}

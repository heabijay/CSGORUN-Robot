using System;
using System.Collections.Generic;
using System.Linq;

namespace CSGORUN_Robot.CSGORUN.WebSocket_DTOs
{
    public class SubscriptionsBuilder
    {
        private int latestId = 1;

        public int? UserId { get; set; } = null;

        private List<Subscription> subscriptions = new();

        private static readonly SubscriptionType[] typesRequiresUserId =
        {
            SubscriptionType.u_noty,
            SubscriptionType.u_ub,
            SubscriptionType.user_bet,
            SubscriptionType.u_,
            SubscriptionType.u_i,
            SubscriptionType.noty,
            SubscriptionType.sound,
            SubscriptionType.withdraw_gift,
        };

        public SubscriptionsBuilder AssignUser(int userId)
        {
            UserId = userId;
            return this;
        }

        public SubscriptionsBuilder Add(params SubscriptionType[] subs)
        {
            foreach (var sub in subs)
            {
                var subStr = sub.ToStringEquivalent();

                if (typesRequiresUserId.Contains(sub))
                {
                    if (UserId == null)
                        throw new ArgumentNullException("Selected subscription requires UserId, which isn't set!");

                    subStr += "#" + UserId.ToString();
                }

                subscriptions.Add(new()
                {
                    method = 1,
                    id = ++latestId,
                    @params = new()
                    {
                        channel = subStr
                    }
                });
            }

            return this;
        }

        public List<Subscription> Build()
        {
            return subscriptions;
        }


        public static SubscriptionsBuilder Create() => new();

        public static SubscriptionsBuilder FromList(List<Subscription> subscriptions)
        {
            return new SubscriptionsBuilder()
            {
                subscriptions = subscriptions,
                latestId = subscriptions.Select(t => t.id).Max()
            };
        }
    }
}

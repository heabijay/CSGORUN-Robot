using System;
using System.Collections.Generic;

namespace CSGORUN_Robot.CSGORUN.DTOs
{
    public class CurrentState
    {
        public User user { get; set; }
        public List<Notification> notifications { get; set; }
        public Game game { get; set; }
        public double? currency { get; set; }
        public SystemMessage systemMessage { get; set; }
        public string centrifugeToken { get; set; }
        public List<Youtuber> youtubers { get; set; }
        public object transaction { get; set; }
        public DateTime? raffleAt { get; set; }
        public bool totalizatorEventsExist { get; set; }
        public bool isGiver { get; set; }
        public List<PaymentMethod> paymentMethods { get; set; }
    }
}

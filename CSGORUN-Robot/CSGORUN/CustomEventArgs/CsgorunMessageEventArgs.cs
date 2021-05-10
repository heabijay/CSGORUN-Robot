using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using System;

namespace CSGORUN_Robot.CSGORUN.CustomEventArgs
{
    public class CsgorunMessageEventArgs : EventArgs
    {
        public ChatPayload Message { get; private set; }
        public SubscriptionType Chat { get; private set; }
        public CsgorunMessageEventArgs(SubscriptionType chat, ChatPayload msg)
        {
            Chat = chat;
            Message = msg;
        }
    }
}

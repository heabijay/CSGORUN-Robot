using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;

namespace CSGORUN_Robot.Services.MessageWrappers
{
    public class RuMessageWrapper : MessageWrapperBase<ChatPayload>
    {
        public RuMessageWrapper(ChatPayload message) : base(message)
        {
            
        }

        
    }
}

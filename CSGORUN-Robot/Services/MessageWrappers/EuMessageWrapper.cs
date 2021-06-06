using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;

namespace CSGORUN_Robot.Services.MessageWrappers
{
    public class EuMessageWrapper : MessageWrapperBase<ChatPayload>
    {
        public EuMessageWrapper(ChatPayload message) : base(message)
        {
            
        }
    }
}

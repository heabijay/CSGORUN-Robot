using CSGORUN_Robot.Twitch.DTOs;

namespace CSGORUN_Robot.Services.MessageWrappers
{
    public class TwitchMessageWrapper : MessageWrapperBase<UserMessage>
    {
        public TwitchMessageWrapper(UserMessage message) : base(message)
        {

        }
    }
}

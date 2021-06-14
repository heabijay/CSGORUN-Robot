using Telega.Rpc.Dto.Types;

namespace CSGORUN_Robot.Services.MessageWrappers
{
    public class TelegramChannelMessageWrapper : MessageWrapperBase<(Message.DefaultTag Message, Chat.ChannelTag Channel)>
    {
        public TelegramChannelMessageWrapper((Message.DefaultTag Message, Chat.ChannelTag Channel) message) : base(message)
        {
        }
    }
}

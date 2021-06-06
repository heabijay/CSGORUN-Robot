namespace CSGORUN_Robot.Services.MessageWrappers
{
    public abstract class MessageWrapperBase<T> : MessagePreWrapperBase<T>
    {
        public MessageWrapperBase(T message) : base(message)
        {
        }

        public new T Message => (T)base.Message;
    }

    public abstract class MessagePreWrapperBase<T> : IMessageWrapper
    {
        public MessagePreWrapperBase(T message)
        {
            Message = message;
        }

        public object Message { get; init; }
    }
}

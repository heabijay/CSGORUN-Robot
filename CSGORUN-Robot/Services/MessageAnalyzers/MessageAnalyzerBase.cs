using CSGORUN_Robot.Services.MessageAnalyzers.Exceptions;
using CSGORUN_Robot.Services.MessageWrappers;
using System.Collections.Generic;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public abstract class MessageAnalyzerBase<T>: IMessageАnalyzer
    {
        public virtual IEnumerable<string> Analyze(IMessageWrapper message)
        {
            if (!TryHandleMessage(message))
                throw new UnsupportedAnalyzerException($"The type {message.GetType()} couldn't be cast to {typeof(T)}.");

            return Analyze((T)message);
        }

        private protected abstract IEnumerable<string> Analyze(T message);

        public virtual bool TryHandleMessage(IMessageWrapper message)
        {
            if (message == null)
                return false;

            try
            {
                _ = (T)message;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

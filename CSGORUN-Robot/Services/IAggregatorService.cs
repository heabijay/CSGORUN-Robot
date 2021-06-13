using CSGORUN_Robot.Services.MessageWrappers;
using System;

namespace CSGORUN_Robot.Services
{
    public interface IAggregatorService
    {
        public event EventHandler<IMessageWrapper> MessageReceived;
        public bool IsActive { get; }
        public void Start();
        public void Stop();
    }
}

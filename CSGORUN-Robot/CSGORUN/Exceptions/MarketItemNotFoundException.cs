using System;

namespace CSGORUN_Robot.CSGORUN.Exceptions
{

    [Serializable]
    public class MarketItemNotFoundException : Exception
    {
        public MarketItemNotFoundException() { }
        public MarketItemNotFoundException(string message) : base(message) { }
        public MarketItemNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected MarketItemNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

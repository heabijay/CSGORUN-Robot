using System;

namespace CSGORUN_Robot.CSGORUN.Exceptions
{

    [Serializable]
    public class BalanceLessTargetException : Exception
    {
        public BalanceLessTargetException() { }
        public BalanceLessTargetException(string message) : base(message) { }
        public BalanceLessTargetException(string message, Exception inner) : base(message, inner) { }
        protected BalanceLessTargetException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

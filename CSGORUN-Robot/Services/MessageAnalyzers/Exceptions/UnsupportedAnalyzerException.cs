using System;

namespace CSGORUN_Robot.Services.MessageAnalyzers.Exceptions
{
    [Serializable]
    public class UnsupportedAnalyzerException : Exception
    {
        public UnsupportedAnalyzerException() { }
        public UnsupportedAnalyzerException(string message) : base(message) { }
        public UnsupportedAnalyzerException(string message, Exception inner) : base(message, inner) { }
        protected UnsupportedAnalyzerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

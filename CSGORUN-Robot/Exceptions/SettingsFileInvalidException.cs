using System;

namespace CSGORUN_Robot.Exceptions
{

    [Serializable]
    public class SettingsFileInvalidException : Exception
    {
        public SettingsFileInvalidException() { }
        public SettingsFileInvalidException(string message) : base(message) { }
        public SettingsFileInvalidException(string message, Exception inner) : base(message, inner) { }
        protected SettingsFileInvalidException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

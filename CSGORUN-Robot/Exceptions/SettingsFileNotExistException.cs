using System;

namespace CSGORUN_Robot.Exceptions
{

    [Serializable]
    public class SettingsFileNotExistException : Exception
    {
        public SettingsFileNotExistException() { }
        public SettingsFileNotExistException(string message) : base(message) { }
        public SettingsFileNotExistException(string message, Exception inner) : base(message, inner) { }
        protected SettingsFileNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

using System;
using System.Net.Http;

namespace CSGORUN_Robot.Exceptions
{

    [Serializable]
    public class HttpRequestRawException : Exception
    {
        public new HttpRequestException InnerException => (HttpRequestException)base.InnerException;

        public HttpContent Content { get; set; }
        public HttpRequestRawException() { }
        public HttpRequestRawException(string message) : base(message) { }
        public HttpRequestRawException(string message, Exception inner) : base(message, inner) { }
    }
}

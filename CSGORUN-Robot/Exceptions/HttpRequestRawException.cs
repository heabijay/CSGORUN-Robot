using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Exceptions
{

    [Serializable]
    public class HttpRequestRawException : HttpRequestException
    {
        public HttpContent Content { get; set; }
        public HttpRequestRawException() { }
        public HttpRequestRawException(string message) : base(message) { }
        public HttpRequestRawException(string message, Exception inner) : base(message, inner) { }
    }
}

using CSGORUN_Robot.Exceptions;
using System.Net.Http;

namespace CSGORUN_Robot.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessage EnsureSuccessStatusCodeRaw(this HttpResponseMessage msg)
        {
            if (msg.IsSuccessStatusCode)
                return msg;

            try
            {
                msg.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestRawException(ex.ToString(), ex) { Content = msg.Content };
            }

            return msg;
        }
    }
}

using CSGORUN_Robot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

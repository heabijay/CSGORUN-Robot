using MihaZupan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public class Proxy
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProxyType Type { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public IWebProxy ToWebProxy()
        {
            return Type switch
            {
                ProxyType.HTTP => new WebProxy(Host, Port) { Credentials = new NetworkCredential(Host, Password) },
                ProxyType.SOCKS5 => new HttpToSocks5Proxy(Host, Port, Username, Password),
                _ => null,
            };
        }
    }
}

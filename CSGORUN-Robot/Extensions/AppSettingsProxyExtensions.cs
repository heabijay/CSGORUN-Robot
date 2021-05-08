using CSGORUN_Robot.AppSettings;
using MihaZupan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Extensions
{
    public static class AppSettingsProxyExtensions
    {
        public static IWebProxy ToProxy(this Proxy proxy)
        {
            return proxy?.Type switch
            {
                ProxyType.HTTP => new WebProxy(proxy.Host, proxy.Port) { Credentials = new NetworkCredential(proxy.Host, proxy.Password) },
                ProxyType.SOCKS5 => new HttpToSocks5Proxy(proxy.Host, proxy.Port, proxy.Username, proxy.Password),
                _ => null,
            };
        }
    }
}

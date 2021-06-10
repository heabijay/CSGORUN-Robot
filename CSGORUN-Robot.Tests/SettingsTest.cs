using CSGORUN_Robot.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace CSGORUN_Robot.Tests
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void Serialize()
        {
            var settings = new AppSettings();
            settings.CSGORUN.Accounts = new List<Account>
            {
                new Account()
                {
                    AuthToken = "none",
                    Proxy = new Proxy()
                    {
                        Type = ProxyType.SOCKS5,
                        Host = "address@gmail.com",
                        Port = 80
                    }
                },

                new Account()
                {
                    AuthToken = "none2",
                }
            };

            File.WriteAllText("settings.json", JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true })) ;
        }
    }
}

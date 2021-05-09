using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public static class SettingsProvider
    {
        private readonly static string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static Settings _Current { get; set; }

        public static Settings Provide()
        {
            if (_Current == null)
                return ProvideAsync().GetAwaiter().GetResult();

            return _Current;
        }

        public static async Task<Settings> ProvideAsync()
        {
            if (_Current == null)
            {
                if (!File.Exists(SettingsPath))
                {
                    using (StreamWriter sw = new(Path.Combine(Path.GetDirectoryName(SettingsPath), "settings.example.json")))
                        await JsonSerializer.SerializeAsync(sw.BaseStream, SettingsExample, new JsonSerializerOptions() { WriteIndented = true });

                    throw new SettingsFileNotExistException("Settings file 'settings.json' not exist. Check an example file 'settings.example.json' and make your own settings file.");
                }

                using (StreamReader sr = new(SettingsPath))
                    _Current = await JsonSerializer.DeserializeAsync<Settings>(sr.BaseStream);
            }

            return _Current;
        }


        private static Settings SettingsExample => new()
        {
            CSGORUN = new()
            {
                Accounts = new()
                {
                    new()
                    {
                        AuthToken = "eyJhbGciOiJIUzI1NiIsInAccount.With.HTTPProxy",
                        Proxy = new()
                        {
                            Host = "Proxy.host.com",
                            Port = 80,
                            Type = ProxyType.HTTP,
                            Password = "pwd",
                            Username = "user",
                        }
                    },
                    new()
                    {
                        AuthToken = "eyJhbGciOiJIUzI1NiIsInAccount.With.SOCKS5Proxy",
                        Proxy = new()
                        {
                            Host = "Proxy.host.com",
                            Port = 80,
                            Type = ProxyType.SOCKS5,
                            Password = "pwd",
                            Username = "user",
                        }
                    },
                    new()
                    {
                        AuthToken = "eyJhbGciOiJIUzI1NiIsInAccount.Without.Proxy"
                    }
                },
                RegexPatterns = new()
                {
                    Default = "Pattern Default. Dont forget about bypass quatation mark using escape char.",
                    EN_Admins = "Pattern for english chat admins. Dont forget about bypass quatation mark using escape char.",
                    RU_Admins = "Pattern for russian chat admins. Dont forget about bypass quatation mark using escape char.",
                }
            },
            Twitch = new()
            {
                Channels = "xQcOW, StRoGo",
                RegexPattern = "Pattern for channel owner messages. Dont forget about bypass quatation mark using escape char."
            }
        };
    }
}

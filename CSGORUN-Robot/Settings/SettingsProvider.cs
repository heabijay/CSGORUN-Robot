using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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

        private static ILogger logger => Log.ForContext(typeof(SettingsProvider));
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
                try
                {
                    using (StreamWriter sw = new(Path.Combine(Path.GetDirectoryName(SettingsPath), "settings.example.json")))
                        await JsonSerializer.SerializeAsync(sw.BaseStream, SettingsExample, new JsonSerializerOptions() { WriteIndented = true });
                }
                catch (Exception ex)
                {
                    logger.Warning("Exception while creating/updating 'settings.example.json': {0}", ex);
                }

                if (!File.Exists(SettingsPath))
                {
                    var ex = new SettingsFileNotExistException("Settings file 'settings.json' not exist. Check an example file 'settings.example.json' and make your own settings file.");
                    logger.Error("Settings file 'settings.json' not found: {0}. Details", ex);
                    throw ex;
                }

                try
                {
                    using (StreamReader sr = new(SettingsPath))
                        _Current = await JsonSerializer.DeserializeAsync<Settings>(sr.BaseStream);
                }
                catch (Exception e)
                {
                    var ex = new SettingsFileInvalidException("Settings file 'settings.json' parse has been failed. Please, check correction with example file 'settings.example.json'.", e);
                    logger.Error("Settings file 'settings.json' scheme is invalid. Details: {0}", ex);
                    throw ex;
                }
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

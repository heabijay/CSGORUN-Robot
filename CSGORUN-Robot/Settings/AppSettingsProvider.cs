using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Settings.Exceptions;
using Serilog;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public static class AppSettingsProvider
    {
        private readonly static string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static AppSettings current { get; set; }

        private static ILogger logger => Log.ForContext(typeof(AppSettingsProvider));
        public static AppSettings Provide()
        {
            if (current == null)
                return ProvideAsync().GetAwaiter().GetResult();

            return current;
        }

        public static async Task<AppSettings> ProvideAsync()
        {
            if (current == null)
            {
                try
                {
                    using StreamWriter sw = new(Path.Combine(Path.GetDirectoryName(SettingsPath), "settings.example.json"));
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
                    using StreamReader sr = new(SettingsPath);
                    current = await JsonSerializer.DeserializeAsync<AppSettings>(sr.BaseStream);
                }
                catch (Exception e)
                {
                    var ex = new SettingsFileInvalidException("Settings file 'settings.json' parse has been failed. Please, check correction with example file 'settings.example.json'.", e);
                    logger.Error("Settings file 'settings.json' scheme is invalid. Details: {0}", ex);
                    throw ex;
                }
            }

            return current;
        }


        private static AppSettings SettingsExample => new AppSettings()
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
                },
                AutoPlaceBet = false
            },
            Twitch = new()
            {
                Channels = "xQcOW, StRoGo",
                RegexPattern = "Pattern for channel owner messages. Dont forget about bypass quatation mark using escape char."
            },
            Telegram = new()
            {
                Notifications = new()
                {
                    BotToken = "1234567890:AAAaaAAaa_AaAAaa-AAaAAAaAAaAaAaAAAA",
                    OwnerId = 0
                }
            }
        };
    }
}

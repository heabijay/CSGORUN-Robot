using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Settings.Exceptions;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Settings
{
    public static class AppSettingsProvider
    {
        private readonly static string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static AppSettings _current { get; set; }
        private static ILogger _logger => Log.ForContext(typeof(AppSettingsProvider));

        public static AppSettings Provide()
        {
            if (_current == null)
                return ProvideAsync().GetAwaiter().GetResult();

            return _current;
        }

        public static async Task<AppSettings> ProvideAsync()
        {
            if (_current == null)
            {
                try
                {
                    using StreamWriter sw = new(Path.Combine(Path.GetDirectoryName(_settingsPath), "settings.example.json"));
                    await JsonSerializer.SerializeAsync(sw.BaseStream, _settingsExample, new JsonSerializerOptions() { WriteIndented = true });
                }
                catch (Exception ex)
                {
                    _logger.Warning("Exception while creating/updating 'settings.example.json': {0}", ex);
                }

                if (!File.Exists(_settingsPath))
                {
                    var ex = new SettingsFileNotExistException("Settings file 'settings.json' not exist. Check an example file 'settings.example.json' and make your own settings file.");
                    _logger.Error("Settings file 'settings.json' not found: {0}. Details", ex);
                    throw ex;
                }

                try
                {
                    using StreamReader sr = new(_settingsPath);
                    _current = await JsonSerializer.DeserializeAsync<AppSettings>(sr.BaseStream);
                    
                    foreach (var account in _current.CSGORUN.Accounts)
                    {
                        account.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(account.AuthToken))
                            {
                                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(_current, new JsonSerializerOptions() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
                            }
                        };
                    }
                }
                catch (Exception e)
                {
                    var ex = new SettingsFileInvalidException("Settings file 'settings.json' parse has been failed. Please, check correction with example file 'settings.example.json'.", e);
                    _logger.Error("Settings file 'settings.json' scheme is invalid. Details: {0}", ex);
                    throw ex;
                }
            }

            return _current;
        }


        private static AppSettings _settingsExample => new AppSettings()
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
                AutoPlaceBet = false,
                PromoCache = new(),
                PromoExclusion = new()
                {
                    "CSGORUN",
                    "YOURUN"
                }
            },
            Twitch = new()
            {
                Channels = "xQcOW, StRoGo",
                RegexPattern = "Pattern for channel owner messages. Dont forget about bypass quatation mark using escape char."
            },
            Telegram = new()
            {
                Bot = new()
                {
                    BotToken = "1234567890:AAAaaAAaa_AaAAaa-AAaAAAaAAaAaAaAAAA",
                    OwnerId = 0
                },
                Aggregator = new()
                {
                    ApiHash = "Your telegram API Hash",
                    ApiId = 0,
                    Channels = new()
                    {
                        new() { Username = "runcsgo", Regex = "Regex Pattern" },
                        new() { Username = "runcsgo2", Regex = "Another Pattern" }
                    }
                }
            }
        };
    }
}

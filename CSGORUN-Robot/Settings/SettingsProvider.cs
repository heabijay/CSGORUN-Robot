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
        private static DateTime lastTimeFileWatcherEventRaised { get; set; } = DateTime.Now;
        private static FileSystemWatcher SettingsWatcher { get; set; }
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

                SettingsWatcher = new FileSystemWatcher(Path.GetDirectoryName(SettingsPath), Path.GetFileName(SettingsPath));
                SettingsWatcher.NotifyFilter = NotifyFilters.LastWrite;
                //SettingsWatcher.Changed += new FileSystemEventHandler(
                //    (s, e) =>
                //    {
                //        _Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
                //        Console.WriteLine("Settings was changed");
                //        Console.WriteLine(JsonSerializer.Serialize(_Current, new JsonSerializerOptions() { WriteIndented = true }));
                //    });
                SettingsWatcher.Changed += (s, e) =>
                {
                    if (DateTime.Now.Subtract(lastTimeFileWatcherEventRaised).TotalMilliseconds < 500)
                        return;

                    lastTimeFileWatcherEventRaised = DateTime.Now;

                    Console.WriteLine(JsonSerializer.Serialize(s, new JsonSerializerOptions() { WriteIndented = true }));
                    var newCurrent = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
                    _Current.Merge(newCurrent);
                    Console.WriteLine("Settings was changed");
                    Console.WriteLine(JsonSerializer.Serialize(_Current, new JsonSerializerOptions() { WriteIndented = true }));
                };
                SettingsWatcher.EnableRaisingEvents = true;
            }

            return _Current;
        }


        private static Settings SettingsExample => new Settings
        {
            CSGORUN = new CSGORUN()
            {
                Accounts = new()
                {
                    new()
                    {
                        AuthToken = "eyJhbGciOiJIUzI1NiIsInAccount.With.Proxy",
                        Proxy = new()
                        {
                            Host = "Proxy.host.com",
                            Port = 80,
                            Type = ProxyType.HTTP,
                            Password = "pwd",
                            Username = "user",
                        }
                    },
                    new Account()
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
            }
        };

        //public static Settings Current
        //{
        //    get
        //    {
        //        if (_Current == null)
        //        {
        //            if (!File.Exists(SettingsPath))
        //            {
        //                Console.WriteLine("Settings created");
        //                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new Settings(), new JsonSerializerOptions() { WriteIndented = true }));

        //            }

        //            _Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));

        //            SettingsWatcher = new FileSystemWatcher(Path.GetDirectoryName(SettingsPath), Path.GetFileName(SettingsPath));
        //            SettingsWatcher.NotifyFilter = NotifyFilters.LastWrite;
        //            //SettingsWatcher.Changed += new FileSystemEventHandler(
        //            //    (s, e) =>
        //            //    {
        //            //        _Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
        //            //        Console.WriteLine("Settings was changed");
        //            //        Console.WriteLine(JsonSerializer.Serialize(_Current, new JsonSerializerOptions() { WriteIndented = true }));
        //            //    });
        //            SettingsWatcher.Changed += (s, e) =>
        //            {
        //                _Current = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
        //                Console.WriteLine("Settings was changed");
        //                Console.WriteLine(JsonSerializer.Serialize(_Current, new JsonSerializerOptions() { WriteIndented = true }));
        //            };
        //            SettingsWatcher.EnableRaisingEvents = true;
        //        }

        //        return _Current;
        //    }
        //}
    }
}

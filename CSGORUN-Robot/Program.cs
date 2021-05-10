using CSGORUN_Robot.Client;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Extensions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CSGORUN_Robot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSGORUN-Robot.log"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, 
                    fileSizeLimitBytes: 25000000)
                .CreateLogger();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(services => SettingsProvider.Provide());
                    services.AddSingleton<TwitchService>();
                    services.AddSingleton<CsgorunService>();
                    services.AddSingleton<List<ClientWorker>>(t =>
                    {
                        var settings = t.GetService<Settings.Settings>();
                        return settings.CSGORUN.Accounts.Select(t => new ClientWorker(t)).ToList();
                    });
                    services.AddSingleton<Worker>();
                })
                .UseSerilog()
                .Build();


            var log = Log.Logger.ForContext<Program>();

            log.Information("Welcome to CSGORUN-Robot!");

            var svc = ActivatorUtilities.CreateInstance<Worker>(host.Services);
            if (!svc.TokenTest())
            {
                log.Fatal("Please, rewrite your tokens to be correct!");
                return;
            }

            svc.StartParse();

            

            //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);


            //var settings = SettingsProvider.Provide();
            new ManualResetEvent(false).WaitOne();

            //var client = new Client.Client(settings.CSGORUN.Accounts[settings.CSGORUN.PrimaryAccountId ?? 0]);
            //try
            //{
            //    var resp = client.httpService.GetCurrentStateAsync().GetAwaiter().GetResult();
            //    Console.WriteLine(JsonSerializer.Serialize(resp, new JsonSerializerOptions() { WriteIndented = true }));
            //}
            //catch (HttpRequestRawException ex)
            //{
            //    var str = ex.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //    Console.WriteLine("Error with content: " + str);
            //}
        }
    }
}

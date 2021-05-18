using CSGORUN_Robot.Client;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Telegram;
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
        public static IServiceProvider ServiceProvider;
        static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;

            var host = CreateHostBuilder(args).Build();
            ServiceProvider = host.Services;
            var log = Log.Logger.ForContext<Program>();

            log.Information("Welcome to CSGORUN-Robot!");

            var svc = ActivatorUtilities.CreateInstance<Worker>(host.Services);
            if (!svc.TokenTest())
            {
                log.Fatal("Please, rewrite your tokens to be correct!");
                return;
            }
            svc.StartParse();

            new ManualResetEvent(false).WaitOne();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);


        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(services => AppSettingsProvider.Provide());
            services.AddSingleton(services => 
            {
                var settings = services.GetService<AppSettings>();
                return new TelegramBotService(
                    services.GetService<ILogger<TelegramBotService>>(),
                    settings.Telegram.Notifications.BotToken,
                    settings.Telegram.Notifications.OwnerId);
            });
            services.AddSingleton<TwitchService>();
            services.AddSingleton<CsgorunService>();
            services.AddSingleton<List<ClientWorker>>(services =>
            {
                var settings = services.GetService<AppSettings>();
                return settings.CSGORUN.Accounts.Select(t => new ClientWorker(t)).ToList();
            });
            services.AddSingleton<Worker>();
        }

        public static void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.AddSerilog();

            // Semilog configuration
            var settings = AppSettingsProvider.Provide();
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSGORUN-Robot.log"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                    fileSizeLimitBytes: 25000000);

            if (settings?.Telegram?.Notifications?.BotToken != null &&
                settings?.Telegram?.Notifications?.OwnerId != null)
                loggerConfig = loggerConfig.WriteTo.Telegram(
                        botToken: settings?.Telegram?.Notifications?.BotToken,
                        chatId: settings?.Telegram?.Notifications?.OwnerId.ToString(),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}

using CSGORUN_Robot.Services;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CommandLine;
using Websocket.Client;

namespace CSGORUN_Robot
{
    class Program
    {
        public static IServiceProvider ServiceProvider;
        public static CommandLineOptions CommandLineOptions = new CommandLineOptions();
        static void Main(string[] args)
        {
            ProcessArgs(args);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.OutputEncoding = Encoding.Unicode;

            var host = CreateHostBuilder(args).Build();
            ServiceProvider = host.Services;
            var log = Log.Logger.ForContext<Program>();

            log.Information("Welcome to CSGORUN-Robot!");

            var svc = ServiceProvider.GetRequiredService<Worker>();
            ServiceProvider.GetRequiredService<TelegramBotService>();
            if (!svc.TokenTest())
            {
                log.Fatal("Please, rewrite your tokens to be correct!");
                return;
            }
            svc.StartParse();

            new ManualResetEvent(false).WaitOne();
        }

        public static void ProcessArgs(IEnumerable<string> args)
        {
            var parserResult = CommandLine.Parser.Default
                .ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(
                    opts => { });
            CommandLineOptions = parserResult.Value;
            
            if (parserResult.Tag == ParserResultType.NotParsed) 
                Environment.Exit(0);
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
                    settings.Telegram.Bot.BotToken,
                    settings.Telegram.Bot.OwnerId);
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
                .MinimumLevel.Override(typeof(WebsocketClient).FullName, Serilog.Events.LogEventLevel.Fatal)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                //.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSGORUN-Robot.log"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                    fileSizeLimitBytes: 25000000);

            if (settings?.Telegram?.Bot?.BotToken != null &&
                settings?.Telegram?.Bot?.OwnerId != null)
                loggerConfig = loggerConfig.WriteTo.Telegram(
                        botToken: settings?.Telegram?.Bot?.BotToken,
                        chatId: settings?.Telegram?.Bot?.OwnerId.ToString(),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error);

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}

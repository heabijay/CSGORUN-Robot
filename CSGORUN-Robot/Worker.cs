using CSGORUN_Robot.Client;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Services.MessageAnalyzers;
using CSGORUN_Robot.Services.MessageAnalyzers.Exceptions;
using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CSGORUN_Robot
{
    public class Worker
    {
        public List<ClientWorker> Clients { get; private set; }
        public List<IAggregatorService> Aggregators { get; private set; }

        private readonly ILogger _log;
        private readonly AppSettings _settings;

        private List<IMessageАnalyzer> _messageАnalyzers;

        public Worker(ILogger<Worker> logger, AppSettings settings)
        {
            _log = logger;
            _settings = settings;

            Clients = settings.CSGORUN.Accounts.Select(t => new ClientWorker(t)).ToList();
            InitializeAnalyzers();
            InitializeAggregators();

            Aggregators.ForEach(t => t.MessageReceived += OnMessageAsync);
        }

        private void InitializeAnalyzers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => string.Equals(t.Namespace, typeof(IMessageАnalyzer).Namespace, StringComparison.Ordinal));
            var filtered = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IMessageАnalyzer))).ToList();

            _messageАnalyzers = filtered.Select(t => (IMessageАnalyzer)Activator.CreateInstance(t)).ToList();
        }

        private void InitializeAggregators()
        {
            Aggregators = new()
            {
                new CsgorunService(Program.ServiceProvider.GetRequiredService<ILogger<CsgorunService>>())
            };

            var isTwitchAvailable = !string.IsNullOrWhiteSpace(_settings.Twitch?.Channels);
            if (isTwitchAvailable)
                Aggregators.Add(new TwitchService(Program.ServiceProvider.GetRequiredService<ILogger<TwitchService>>()));

            var isTelegramAvailable = !string.IsNullOrWhiteSpace(_settings.Telegram?.Aggregator?.ApiHash) && _settings.Telegram?.Aggregator?.ApiId > 0;
            if (isTelegramAvailable)
                Aggregators.Add(new TelegramService(Program.ServiceProvider.GetRequiredService<ILogger<TelegramService>>()));
        }

        public bool TokenTest()
        {
            _log.LogInformation("[{0}] Running", nameof(TokenTest));

            bool isSuccess = true;

            Parallel.For(0, Clients.Count, i =>
            {
                try
                {
                    var state = Clients[i].HttpService.GetCurrentStateAsync().GetAwaiter().GetResult();
                    _log.LogInformation("[{0}#{1}] {2} - {3}$", nameof(TokenTest), i, state.user.name, state.user.balance);
                }
                catch (HttpRequestRawException ex)
                {
                    isSuccess = false;
                    var inner = ex.InnerException;
                    _log.LogError("[{0}#{1}] {2} - {3}", nameof(TokenTest), i, (int)inner?.StatusCode, inner?.StatusCode);
                }
            });

            return isSuccess;
        }

        public void StartParse()
        {
            Aggregators.ForEach(t => t.Start());
        }

        public void StopParse()
        {
            Aggregators.ForEach(t => t.Stop());
        }

        public void EnqueuePromo(string promo)
        {
            foreach (var client in Clients)
            {
                client.EnqueuePromo(promo);
            }
        }

        private void OnMessageAsync(object sender, IMessageWrapper data)
        {
            var analyzer = GetАnalyzer(data);
            var promos = analyzer.Analyze(data);
            
            if (promos?.Count() > 0)
            {
                _log.LogInformation("{0} promo found: {1}", analyzer.GetType().Name, string.Join("; ", promos));

                foreach (var promo in promos)
                {
                    EnqueuePromo(promo);
                }
            }
        }

        private IMessageАnalyzer GetАnalyzer(IMessageWrapper message)
        {
            foreach (var analyzer in _messageАnalyzers)
            {
                if (analyzer.TryHandleMessage(message))
                {
                    return analyzer;
                }
            }

            throw new UnsupportedAnalyzerException($"Supported analyzer not found for type {message.GetType()}!");
        }
    }
}

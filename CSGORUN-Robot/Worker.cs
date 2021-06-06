using CSGORUN_Robot.Client;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services;
using CSGORUN_Robot.Services.MessageAnalyzers;
using CSGORUN_Robot.Services.MessageAnalyzers.Exceptions;
using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
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
        private readonly ILogger log;
        private readonly AppSettings settings;
        private readonly TelegramBotService _telegramBotService;
        public List<ClientWorker> Clients { get; private set; }
        public List<IParserService> Parsers { get; private set; } = new List<IParserService>();

        private List<IMessageАnalyzer> _messageАnalyzers;

        public Worker(ILogger<Worker> logger, AppSettings settings, TelegramBotService telegramBotService, TwitchService twitch, CsgorunService csgorun, List<ClientWorker> clientWorkers)
        {
            log = logger;
            this.settings = settings;
            _telegramBotService = telegramBotService;
            InitializeAnalyzers();

            Clients = clientWorkers;
            Parsers.Add(twitch);
            Parsers.Add(csgorun);

            Parsers.ForEach(t => t.MessageReceived += OnMessageAsync);
            csgorun.GameStarted += OnGameStarted;
        }

        private void InitializeAnalyzers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => string.Equals(t.Namespace, typeof(IMessageАnalyzer).Namespace, StringComparison.Ordinal));
            var filtered = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IMessageАnalyzer))).ToList();

            _messageАnalyzers = filtered.Select(t => (IMessageАnalyzer)Activator.CreateInstance(t)).ToList();
        }

        public bool TokenTest()
        {
            log.LogInformation("[{0}] Running", nameof(TokenTest));

            bool isSuccess = true;

            Parallel.For(0, Clients.Count, i =>
            {
                try
                {
                    var state = Clients[i].HttpService.GetCurrentStateAsync().GetAwaiter().GetResult();
                    log.LogInformation("[{0}#{1}] {2} - {3}$", nameof(TokenTest), i, state.user.name, state.user.balance);
                }
                catch (HttpRequestRawException ex)
                {
                    isSuccess = false;
                    var inner = ex.InnerException;
                    log.LogError("[{0}#{1}] {2} - {3}", nameof(TokenTest), i, (int)inner?.StatusCode, inner?.StatusCode);
                }
            });

            return isSuccess;
        }

        public void StartParse()
        {
            Parsers.ForEach(t => t.Start());
        }

        public void StopParse()
        {
            Parsers.ForEach(t => t.Stop());
        }

        public void EnqueuePromo(string promo)
        {
            foreach (var client in Clients)
            {
                client.EnqueuePromo(promo);
            }
        }

        private async void OnMessageAsync(object sender, IMessageWrapper data)
        {
            var analyzer = GetАnalyzer(data);
            var promos = analyzer.Analyze(data);
            
            if (promos?.Count() > 0)
            {
                log.LogInformation("{0} promo found: {1}", analyzer.GetType().Name, string.Join("; ", promos));

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

        private void OnGameStarted(object sender, EventArgs e)
        {
            
        }
    }
}

﻿using CSGORUN_Robot.Client;
using CSGORUN_Robot.CSGORUN.CustomEventArgs;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot
{
    public class Worker
    {
        private readonly ILogger log;
        public List<ClientWorker> Clients { get; private set; }
        public List<IParserService> Parsers { get; private set; } = new List<IParserService>();

        public Worker(ILogger<Worker> logger, Settings.Settings settings, TwitchService twitch, CsgorunService csgorun, List<ClientWorker> clientWorkers)
        {
            log = logger;

            Clients = clientWorkers;
            Parsers.Add(twitch);
            Parsers.Add(csgorun);

            Parsers.ForEach(t => t.MessageReceived += OnMessageAsync);
            csgorun.GameStarted += OnGameStarted;
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

        private async void OnMessageAsync(object sender, object data)
        {
            //if (sender.GetType() == typeof(CsgorunService))
            //{
            //    var d = (CsgorunMessageEventArgs)data;
            //    if (d.Chat == CSGORUN.WebSocket_DTOs.SubscriptionType.game)
            //    {
            //        d.
            //    }
            //}
        }

        private void OnGameStarted(object sender, EventArgs e)
        {
            log.LogInformation("Game Started!");
        }
    }
}
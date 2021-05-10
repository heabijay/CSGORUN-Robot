using CSGORUN_Robot.Client;
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
        private ILogger log;
        private List<ClientUnit> clients { get; set; }
        private List<IParserService> parsers { get; set; } = new List<IParserService>();

        public Worker(ILogger<Worker> logger, Settings.Settings settings, TwitchService service)
        {
            log = logger;

            clients = settings.CSGORUN.Accounts.Select(t => new ClientUnit(t)).ToList();
            parsers.Add(service);
        }

        public async Task<bool> TokenTestAsync()
        {
            log.LogInformation("[{0}] Running", nameof(TokenTestAsync));

            bool isSuccess = true;

            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    var state = await clients[i].HttpService.GetCurrentStateAsync();
                    log.LogInformation("[{0}#{1}] {2} - {3}$", nameof(TokenTestAsync), i, state.user.name, state.user.balance);
                }
                catch (HttpRequestRawException ex)
                {
                    isSuccess = false;
                    var innerEx = ex.InnerException as HttpRequestException;
                    log.LogError("[{0}#{1}] {2} - {3}", nameof(TokenTestAsync), i, (int)innerEx?.StatusCode, innerEx?.StatusCode);
                }
            }

            return isSuccess;
        }
    }
}

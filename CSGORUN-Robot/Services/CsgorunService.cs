using CSGORUN_Robot.Client;
using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using CSGORUN_Robot.CustomEventArgs;
using CSGORUN_Robot.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace CSGORUN_Robot.Services
{
    public class CsgorunService : IParserService
    {
        private string subscriptionsStr;

        private ILogger log;

        private ClientWorker clientWorker;

        public bool IsActive => ws != null && ws.IsRunning;

        private static Timer timer = null;
        private WebsocketClient ws = null;

        public event EventHandler<object> MessageReceived;
        public event EventHandler GameStarted;

        public CsgorunService(ILogger<CsgorunService> logger, Settings.Settings settings, List<ClientWorker> clientWorkers)
        {
            log = logger;
            clientWorker = clientWorkers[settings.CSGORUN.PrimaryAccountIndex];

            var subscriptions = SubscriptionsBuilder
                .Create()
                .Add(SubscriptionType.game)
                .Add(SubscriptionType.c_en)
                .Add(SubscriptionType.c_ru)
                .Build();

            subscriptionsStr = string.Join('\n', subscriptions.Select(t => JsonSerializer.Serialize(t)));

            ws = new WebsocketClient(new Uri(CSGORUN.Routing.WebSocket), () => new ClientWebSocket()
            {
                Options =
                {
                    Proxy = clientWorker?.Account?.Proxy?.ToWebProxy()
                }
            })
            {
                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
            };
            ws.MessageReceived.Subscribe(OnMessageAsync);
            ws.DisconnectionHappened.Subscribe(OnDisconnect);
            ws.ReconnectionHappened.Subscribe(OnReconnect);
        }

        public void Start()
        {
            ws.Start();

            timer = new Timer(
                new TimerCallback(obj =>
                {
                    ws?.Send(subscriptionsStr);
                }),
                null,
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(15)
            );
        }

        public void Stop()
        {
            if (ws != null && ws.IsRunning)
                ws.Stop(WebSocketCloseStatus.Empty, "Stopped.");
            timer.Dispose();
        }

        private void OnDisconnect(DisconnectionInfo disc)
        {
            log.LogInformation("[WS] Disconnected due {0}", disc.Type);

            if (disc.Type == DisconnectionType.ByServer)
                ws.Reconnect();
        }
        private void OnReconnect(ReconnectionInfo recon)
        {
            ws?.Send(@"{""params"":{""token"":""" + clientWorker?.HttpService?.LastCurrentState?.centrifugeToken + @"""},""id"":1}");

            log.LogInformation("[WS] Connected: {0}", recon.Type);
        }

        private static bool IsUnauthorizedMessage(string msg) => msg.EndsWith("\"error\":{\"code\":101,\"message\":\"unauthorized\"}}");
        private static bool IsAuthorizedMessage(string msg) => msg.StartsWith("{\"id\":1,\"result\":{\"client\":\"");

        private async void OnMessageAsync(ResponseMessage m)
        {
            if (m.MessageType == WebSocketMessageType.Text)
            {
                string[] dataCollection = m.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string dataRaw in dataCollection)
                {
                    if (IsUnauthorizedMessage(dataRaw))
                    {
                        log.LogInformation("[WS] Message returns unauthorized.");
                        Stop();

                        do
                        {
                            await Task.Delay(2500);
                            try
                            {
                                await clientWorker.HttpService.GetCurrentStateAsync();
                                break;
                            }
                            catch (HttpRequestRawException ex) when ((ex.InnerException as HttpRequestException).StatusCode == HttpStatusCode.Unauthorized)
                            {
                                log.LogCritical("[WS] CurrentState request returns unauthorized!");
                                return;
                            }
                            catch (Exception ex)
                            {
                                log.LogError(ex, "[WS] CurrentState request exception.");
                            }
                        }
                        while (true);

                        Start();
                        return;
                    }
                    else if (IsAuthorizedMessage(dataRaw))
                    {
                        ws.Send(subscriptionsStr);
                        log.LogInformation("[WS] Subscriptions sent.");
                        return;
                    }

                    try
                    {
                        var data = JsonSerializer.Deserialize<SuccessResponse<object>>(dataRaw);

                        var channel = SubscriptionTypeExtensions.Parse(data.result.channel);

                        if (channel is (SubscriptionType.c_ru or SubscriptionType.c_en))
                        {
                            var msg = JsonSerializer.Deserialize<ChatData>(data.result.data.data.ToString()).payload;

                            log.LogDebug("[{0}] {1}: {2}", data.result.channel, msg?.user?.nickname, msg?.message);

                            MessageReceived?.Invoke(this, new CsgorunMessageEventArgs(channel, msg));
                        }
                        else if (channel is SubscriptionType.game)
                        {
                            var type = ((JsonElement)data.result.data.data).GetProperty("type").GetString();
                            if (type.Equals("start", StringComparison.OrdinalIgnoreCase))
                                GameStarted?.Invoke(this, new EventArgs());

                        }
                    }
                    catch (Exception ex)
                    {
                        //log.LogWarning(ex, "[WS] Something went wrong while parsing the message.");
                    }
                }
            }
        }
    }
}

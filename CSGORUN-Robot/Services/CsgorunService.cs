using CSGORUN_Robot.Client;
using CSGORUN_Robot.CSGORUN.CustomEventArgs;
using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Settings;
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
        private readonly string subscriptionsStr;

        private readonly ILogger log;

        private ClientWorker _currentClientWorker;
        private readonly List<ClientWorker> _clientWorkers;

        public bool IsActive => ws != null && ws.IsRunning;

        private static Timer timer = null;
        private WebsocketClient ws = null;

        public event EventHandler<object> MessageReceived;
        public event EventHandler GameStarted;

        public CsgorunService(ILogger<CsgorunService> logger, List<ClientWorker> clientWorkers)
        {
            log = logger;
            _clientWorkers = clientWorkers;

            var subscriptions = SubscriptionsBuilder
                .Create()
                .Add(SubscriptionType.game)
                .Add(SubscriptionType.c_en)
                .Add(SubscriptionType.c_ru)
                .Build();

            subscriptionsStr = string.Join('\n', subscriptions.Select(t => JsonSerializer.Serialize(t)));

            WebSocketInit();
        }

        public void WebSocketInit()
        {
            ws?.Dispose();

            _currentClientWorker = _clientWorkers.FirstOrDefault(t => t.HttpService.IsAuthorized);
            if (_currentClientWorker == null)
            {
                log.LogCritical("[WS] Available account doesn't found! The service will be stopped!");
                return;
            }

            ws = new WebsocketClient(new Uri(CSGORUN.Routing.WebSocket), () => new ClientWebSocket()
            {
                Options =
                {
                    Proxy = _currentClientWorker?.Account?.Proxy?.ToWebProxy()
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
            ws?.Send(@"{""params"":{""token"":""" + _currentClientWorker?.HttpService?.LastCurrentState?.centrifugeToken + @"""},""id"":1}");

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
                                await _currentClientWorker.HttpService.GetCurrentStateAsync();
                                break;
                            }
                            catch (HttpRequestRawException ex) when (ex.InnerException.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                WebSocketInit();
                                if (ws != null) Start();
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
                        var data = JsonSerializer.Deserialize<SuccessResponse<JsonDocument>>(dataRaw);

                        if (data?.result?.channel != null)
                        {
                            var channel = SubscriptionTypeExtensions.Parse(data.result.channel);

                            if (channel is (SubscriptionType.c_ru or SubscriptionType.c_en))
                            {
                                var chatData = JsonSerializer.Deserialize<ChatData>(data.result.data.data.RootElement.ToString());
                                var msg = chatData.payload;

                                if (chatData.a.Equals("new", StringComparison.OrdinalIgnoreCase))
                                {
                                    log.LogDebug("[{0}] {1}: {2}", data.result.channel, msg?.user?.nickname, msg?.message);

                                    MessageReceived?.Invoke(this, new CsgorunMessageEventArgs(channel, msg));
                                }
                            }
                            else if (channel is SubscriptionType.game)
                            {
                                var type = data.result.data.data.RootElement.GetProperty("type").GetString();
                                if (type.Equals("start", StringComparison.OrdinalIgnoreCase))
                                    GameStarted?.Invoke(this, new EventArgs());

                            }
                        }
                    }
                    catch
                    {
                        //log.LogWarning(ex, "[WS] Something went wrong while parsing the message.");
                    }
                }
            }
        }
    }
}

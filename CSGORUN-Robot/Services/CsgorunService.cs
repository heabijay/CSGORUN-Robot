using CSGORUN_Robot.Client;
using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using CSGORUN_Robot.Exceptions;
using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
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
    public class CsgorunService : IAggregatorService
    {
        public bool IsActive => _ws != null && _ws.IsRunning;

        public event EventHandler<IMessageWrapper> MessageReceived;
        public event EventHandler CrashGameStarted;
        public event EventHandler RouletteGameStarted;

        private readonly string _subscriptionsStr;

        private readonly ILogger _log;

        private ClientWorker _currentClientWorker;

        private static Timer _timer = null;
        private WebsocketClient _ws = null;
        

        public CsgorunService(ILogger<CsgorunService> logger)
        {
            _log = logger;

            var subscriptions = SubscriptionsBuilder
                .Create()
                .Add(SubscriptionType.game)
                .Add(SubscriptionType.c_en)
                .Add(SubscriptionType.c_ru)
                .Add(SubscriptionType.roulette)
                .Build();

            _subscriptionsStr = string.Join('\n', subscriptions.Select(t => JsonSerializer.Serialize(t)));
        }

        public void WebSocketInit()
        {
            _ws?.Dispose();

            var worker = Program.ServiceProvider.GetRequiredService<Worker>();

            _currentClientWorker = worker.Clients.FirstOrDefault(t => t.HttpService.IsAuthorized);
            if (_currentClientWorker == null)
            {
                _log.LogCritical("[WS] Available account doesn't found! The service will be stopped!");
                return;
            }

            _ws = new WebsocketClient(new Uri(CSGORUN.Routing.WebSocket), () => new ClientWebSocket()
            {
                Options =
                {
                    Proxy = _currentClientWorker?.Account?.Proxy?.ToWebProxy()
                }
            })
            {
                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
            };
            _ws.MessageReceived.Subscribe(OnMessageAsync);
            _ws.DisconnectionHappened.Subscribe(OnDisconnect);
            _ws.ReconnectionHappened.Subscribe(OnReconnect);
        }

        public void Start()
        {
            if (_ws == null) WebSocketInit();

            _ws.Start();

            _timer = new Timer(
                new TimerCallback(obj =>
                {
                    _ws?.Send(_subscriptionsStr);
                }),
                null,
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(15)
            );
        }

        public void Stop()
        {
            if (_ws != null && _ws.IsRunning)
                _ws.Stop(WebSocketCloseStatus.Empty, "Stopped.");
            _timer.Dispose();
        }

        private void OnDisconnect(DisconnectionInfo disc)
        {
            _log.LogInformation("[WS] Disconnected due {0}", disc.Type);

            if (disc.Type == DisconnectionType.ByServer)
                _ws.Reconnect();
        }
        private void OnReconnect(ReconnectionInfo recon)
        {
            _ws?.Send(@"{""params"":{""token"":""" + _currentClientWorker?.HttpService?.LastCurrentState?.centrifugeToken + @"""},""id"":1}");

            _log.LogInformation("[WS] Connected: {0}", recon.Type);
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
                        _log.LogInformation("[WS] Message returns unauthorized.");
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
                                if (_ws != null) Start();
                                return;
                            }
                            catch (Exception ex)
                            {
                                _log.LogError(ex, "[WS] CurrentState request exception.");
                            }
                        }
                        while (true);

                        Start();
                        return;
                    }
                    else if (IsAuthorizedMessage(dataRaw))
                    {
                        _ws.Send(_subscriptionsStr);
                        _log.LogInformation("[WS] Subscriptions sent.");
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
                                    _log.LogDebug("[{0}] {1}: {2}", data.result.channel, msg?.user?.nickname, msg?.message);

                                    MessageReceived?.Invoke(this, channel == SubscriptionType.c_ru ? new RuMessageWrapper(msg) as IMessageWrapper : new EuMessageWrapper(msg) as IMessageWrapper);
                                }
                            }
                            else if (channel is SubscriptionType.game)
                            {
                                var type = data.result.data.data.RootElement.GetProperty("type").GetString();
                                if (type.Equals("start", StringComparison.OrdinalIgnoreCase))
                                    CrashGameStarted?.Invoke(this, new EventArgs());
                            }
                            else if (channel is SubscriptionType.roulette)
                            {
                                var type = data.result.data.data.RootElement.GetProperty("round").GetProperty("status").GetInt32();
                                if (type.Equals(1))
                                    RouletteGameStarted?.Invoke(this, new EventArgs());
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

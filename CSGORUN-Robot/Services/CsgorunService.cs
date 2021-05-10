//using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.WebSockets;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Websocket.Client;
//using Websocket.Client.Models;

//namespace CSGORUN_Robot.Services
//{
//    public class CsgorunService : IParserService
//    {
//        private string subscriptionsStr;

//        private ILogger log { get; set; }

//        public bool IsActive => ws != null && ws.IsRunning;

//        private static Timer timer = null;
//        private WebsocketClient ws = null;

//        public event EventHandler<object> MessageReceived;

//        public CsgorunService(ILogger<CsgorunService> logger, Settings.Settings settings)
//        {
//            log = logger;

//            var subscriptions = SubscriptionsBuilder
//                .Create()
//                .Add(SubscriptionType.game)
//                .Add(SubscriptionType.c_en)
//                .Add(SubscriptionType.c_ru)
//                .Build();

//            subscriptionsStr = string.Join('\n', JsonSerializer.Serialize(subscriptions));

//            ws = new WebsocketClient(new Uri(CSGORUN.Routing.WebSocket), WebSocketClientFactory)
//            {
//                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
//            };
//            ws.MessageReceived.Subscribe(OnMessage);
//            ws.DisconnectionHappened.Subscribe(OnDisconnect);
//            ws.ReconnectionHappened.Subscribe(OnReconnect);
//        }

//        public static Func<ClientWebSocket> WebSocketClientFactory = new Func<ClientWebSocket>(() => new ClientWebSocket
//        {
//            Options =
//                {
//                    Proxy = DefaultProxy
//                }
//        });

//        public void Start()
//        {
            
//            ws.Start();

//            timer = new Timer(
//                new TimerCallback(obj =>
//                {
//                    ws.Send(subscriptionsStr);
//                }),
//                null,
//                TimeSpan.FromMinutes(15),
//                TimeSpan.FromMinutes(15)
//            );
//        }

//        public void Stop()
//        {
//            if (ws != null && ws.IsRunning) 
//                ws.Stop(WebSocketCloseStatus.Empty, "Stopped.");
//            timer.Dispose();
//        }

//        private void OnDisconnect(DisconnectionInfo disc)
//        {
//            log.LogInformation("Disconnected due {0}", disc.Type);

//            if (disc.Type == DisconnectionType.ByServer)
//                ws.Reconnect();
//        }
//        private void OnReconnect(ReconnectionInfo recon)
//        {
//            Retry:
//            try
//            {
//                if (token == null)
//                {
//                    token = CSGORUN.API.GetResponse(Program.Settings.Link_CurrentState).CentrifugeToken;
//                }
//            }
//            catch (WebException ex)
//            {
//                int statusCode = (int)(ex.Response as HttpWebResponse).StatusCode;
//                switch (statusCode)
//                {
//                    case 401:
//                        Console.WriteLine("[WebSocket] Getting current state: | (401) Unauthorized.");
                        
//                        Stop();
//                        return;
//                    case 429:
//                        Console.WriteLine("[WebSocket] Getting current state: | (429) Too Many Requests. Retrying..");
//                        Thread.Sleep(5000);
//                        goto Retry;
//                    default:
//                        Console.WriteLine("[WebSocket] Getting current state: | " + statusCode);
//                        Thread.Sleep(5000);
//                        goto Retry;
//                };

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("[WebSocket] Error while getting current state: " + ex.Message);
//                Thread.Sleep(2500);
//                goto Retry;
//            }

//            ws.Send(@"{""params"":{""token"":""" + token + @"""},""id"":1}");

//            log.LogInformation("Connected: {0}", recon.Type);

//            token = null;
//        }

//        private void OnMessage(ResponseMessage m)
//        {
//            if (m.MessageType == WebSocketMessageType.Text)
//            {
//                try
//                {
//                    string[] dataCollection = m.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//                    foreach (string dataRaw in dataCollection)
//                    {
//                        if (dataRaw.EndsWith("\"error\":{\"code\":101,\"message\":\"unauthorized\"}}"))
//                        {
//                            Thread.Sleep(2000);
//                            ws.Reconnect();
//                            return;
//                        }
//                        else if (dataRaw.StartsWith("{\"id\":1,\"result\":{\"client\":\""))
//                        {
//                            ws.Send(@"{""method"":1,""params"":{ ""channel"":""c-ru""},""id"":2}" + '\n' + @"{""method"":1,""params"":{ ""channel"":""c-en""},""id"":3}");
//                            Console.WriteLine("[WebSocket] Methods and params sent.");
//                            return;
//                        }

//                        CSGORUN_WS_RESPONSE data = JsonConvert.DeserializeObject<CSGORUN_WS_RESPONSE>(dataRaw);

//                        // SWITCH OF RESPONSE TYPE
//                        switch (data?.Result?.Channel)
//                        {
//                            // CHAT
//                            case "c-en":
//                            case "c-ru":
//                                MessageReceived?.Invoke(this, )
//                                break;
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"[ERROR]\t{ex.Message} |>| Input Data: {m.Text}");
//                    File.AppendAllText(Program.Settings.LogFile, $"\n{DateTime.Now} | [ERROR] {ex.Message} |>| Input Data: {m.Text}");
//                    Console.ResetColor();
//                }
//            }

//        }
//    }
//}
//}

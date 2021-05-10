﻿using CSGORUN_Robot.Settings;
using CSGORUN_Robot.Twitch.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace CSGORUN_Robot.Services
{
    public class TwitchService : IParserService
    {
        private List<string> channels { get; set; } = new List<string>();
        private ILogger log { get; set; }

        public event EventHandler<object> MessageReceived;

        private WebsocketClient ws { get; set; }

        public bool IsActive => ws != null && ws.IsRunning;

        private Timer timer = null;

        public TwitchService(ILogger<TwitchService> logger, Settings.Settings settings)
        {
            log = logger;
            channels = settings?.Twitch?.Channels?.Split(',')?.Select(t => t.Trim().Trim('@', '#').ToLower())?.ToList();

            ws = new WebsocketClient(new Uri(Twitch.Routing.WebSocketUrl))
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
            };
            ws.MessageReceived.Subscribe(OnMessage);
            ws.DisconnectionHappened.Subscribe(OnDisconnect);
            ws.ReconnectionHappened.Subscribe(OnReconnect);
        }

        public void Start()
        {
            ws.Start();

            timer = new Timer(
                new TimerCallback(obj => { ws.Send("PING"); }),
                null,
                0,
                (int)TimeSpan.FromMinutes(5).TotalMilliseconds
            );
        }

        public void Stop()
        {
            if (ws != null && ws.IsRunning)
                ws.Stop(WebSocketCloseStatus.Empty, "Stopped.");
            timer?.Dispose();
        }

        private void OnDisconnect(DisconnectionInfo disc)
        {
            log.LogInformation("Disconnected due {0}", disc.Type);

            if (disc.Type == DisconnectionType.ByServer)
            {
                ws.Reconnect();
            }
        }
        private void OnReconnect(ReconnectionInfo recon)
        {
            ws.SendInstant("CAP REQ :twitch.tv/tags twitch.tv/commands");
            ws.SendInstant("PASS SCHMOOPIIE");
            ws.SendInstant("NICK justinfan6288");
            ws.SendInstant("USER justinfan6288 8 * :justinfan6288");

            foreach (string channel in channels ?? Enumerable.Empty<string>())
            {
                ws.Send("JOIN #" + channel);
            }

            log.LogInformation("Connected: {0}", recon.Type);
        }

        private void OnMessage(ResponseMessage m)
        {
            if (m.MessageType == WebSocketMessageType.Text)
            {
                try
                {
                    if (m.Text.StartsWith("PING"))
                    {
                        ws.Send("PONG");
                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));
                            ws?.Send("PING");
                        });
                        return;
                    }
                    else if (m.Text.StartsWith("PONG")) return;

                    var msg = ParseMessage(m.Text);
                    if (msg != null)
                    {
                        log.LogDebug("[#{0}] {1}: {2}", msg.Channel, msg.Nickname, msg.Message);
                        MessageReceived?.Invoke(this, msg);
                    }
                }
                catch { }
            }
        }

        private UserMessage ParseMessage(string message)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] dataraw = message.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < dataraw.Length; i++)
            {
                try
                {
                    int IndexEq = dataraw[i].IndexOf('=');
                    string Key = dataraw[i].Substring(0, IndexEq);
                    string Value = dataraw[i].Substring(IndexEq + 1);

                    switch (Key)
                    {
                        case "user-type":
                            Value = Value.Substring(Value.IndexOf("PRIVMSG") + 9);
                            string dataChannel = Value.Substring(0, Value.IndexOf(' '));
                            string dataMessage = Value.Substring(Value.IndexOf(':') + 1);

                            data.Add("channel", dataChannel);
                            data.Add("message", dataMessage.Trim());
                            break;
                        default:
                            data.Add(Key, Value);
                            break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            // Chat Message
            if (data.TryGetValue("channel", out string channel) && data.TryGetValue("message", out string msg) && data.TryGetValue("display-name", out string nickname))
            {
                return new UserMessage
                {
                    Channel = channel,
                    Message = msg,
                    Nickname = nickname
                };
            }

            return null;
        }
    }
}

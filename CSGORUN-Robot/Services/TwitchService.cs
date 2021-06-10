using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using CSGORUN_Robot.Twitch.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace CSGORUN_Robot.Services
{
    public class TwitchService : IParserService
    {
        public bool IsActive => _ws != null && _ws.IsRunning;
        public event EventHandler<IMessageWrapper> MessageReceived;

        private List<string> _channels { get; set; } = new List<string>();
        private ILogger _log { get; set; }
        private WebsocketClient _ws { get; set; }
        private Timer _timer = null;

        public TwitchService(ILogger<TwitchService> logger, AppSettings settings)
        {
            _log = logger;
            _channels = settings?.Twitch?.Channels?.Split(',')?.Select(t => t.Trim().Trim('@', '#').ToLower())?.ToList();

            _ws = new WebsocketClient(new Uri(Twitch.Routing.WebSocketUrl))
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(5)
            };
            _ws.MessageReceived.Subscribe(OnMessage);
            _ws.DisconnectionHappened.Subscribe(OnDisconnect);
            _ws.ReconnectionHappened.Subscribe(OnReconnect);
        }

        public void Start()
        {
            _ws.Start();

            _timer = new Timer(
                new TimerCallback(obj => { _ws.Send("PING"); }),
                null,
                0,
                (int)TimeSpan.FromMinutes(5).TotalMilliseconds
            );
        }

        public void Stop()
        {
            if (_ws != null && _ws.IsRunning)
                _ws.Stop(WebSocketCloseStatus.Empty, "Stopped.");
            _timer?.Dispose();
        }

        private void OnDisconnect(DisconnectionInfo disc)
        {
            _log.LogInformation("[WS-T] Disconnected due {0}", disc.Type);

            if (disc.Type == DisconnectionType.ByServer)
            {
                _ws.Reconnect();
            }
        }
        private void OnReconnect(ReconnectionInfo recon)
        {
            _ws.SendInstant("CAP REQ :twitch.tv/tags twitch.tv/commands");
            _ws.SendInstant("PASS SCHMOOPIIE");
            _ws.SendInstant("NICK justinfan6288");
            _ws.SendInstant("USER justinfan6288 8 * :justinfan6288");

            foreach (string channel in _channels ?? Enumerable.Empty<string>())
            {
                _ws.Send("JOIN #" + channel);
            }

            _log.LogInformation("[WS-T] Connected: {0}", recon.Type);
        }

        private void OnMessage(ResponseMessage m)
        {
            if (m.MessageType == WebSocketMessageType.Text)
            {
                try
                {
                    if (m.Text.StartsWith("PING"))
                    {
                        _ws.Send("PONG");
                        Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));
                            _ws?.Send("PING");
                        });
                        return;
                    }
                    else if (m.Text.StartsWith("PONG")) return;

                    var msg = ParseMessage(m.Text);
                    if (msg != null)
                    {
                        _log.LogDebug("[#{0}] {1}: {2}", msg.Channel, msg.Nickname, msg.Message);
                        MessageReceived?.Invoke(this, new TwitchMessageWrapper(msg));
                    }
                }
                catch { }
            }
        }

        private static UserMessage ParseMessage(string message)
        {
            Dictionary<string, string> data = new();
            string[] dataraw = message.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < dataraw.Length; i++)
            {
                try
                {
                    int indexEq = dataraw[i].IndexOf('=');
                    string Key = dataraw[i].Substring(0, indexEq);
                    string Value = dataraw[i][(indexEq + 1)..];

                    switch (Key)
                    {
                        case "user-type":
                            Value = Value[(Value.IndexOf("PRIVMSG") + 9)..];
                            string dataChannel = Value.Substring(0, Value.IndexOf(' '));
                            string dataMessage = Value[(Value.IndexOf(':') + 1)..];

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

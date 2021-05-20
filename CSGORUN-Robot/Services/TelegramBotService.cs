using CSGORUN_Robot.Services.TelegramBotCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services
{
    public class TelegramBotService
    {
        TelegramBotClient _bot;
        int _ownerId;
        ILogger<TelegramBotService> _log;

        public List<ITelegramBotCommand> Commands;

        public TelegramBotService(ILogger<TelegramBotService> logger, string botToken, int ownerId)
        {
            _log = logger;
            _bot = new TelegramBotClient(botToken);

            _ownerId = ownerId;

            InitializeCommands();
            _bot.OnMessage += OnMessageReceivedAsync;
            _bot.StartReceiving();

            _log.LogInformation("[{0}] Initialized", typeof(TelegramBotService).Name);
        }

        ~TelegramBotService()
        {
            _bot.StopReceiving();
        }

        private void InitializeCommands()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => string.Equals(t.Namespace, typeof(ITelegramBotCommand).Namespace, System.StringComparison.Ordinal));
            var filtered = types.Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ITelegramBotCommand))).ToList();

            Commands = filtered.Select(t => (ITelegramBotCommand)Activator.CreateInstance(t, _bot, this)).ToList();
        }

        private async void OnMessageReceivedAsync(object sender, MessageEventArgs e)
        {
            _log.LogDebug("[{0}::{1}] {2} {3} (ID:{4}): {5}",
                typeof(TelegramBotService).Name,
                nameof(OnMessageReceivedAsync),
                e.Message.From.FirstName,
                e.Message.From.LastName,
                e.Message.From.Id,
                e.Message.Text);

            var msg = e.Message;
            if (msg.From.Id == _ownerId)
            {
                if (msg.Text.StartsWith('/'))
                {
                    var spaceIndex = msg.Text.IndexOf(' ');
                    string cmd;
                    if (spaceIndex < 0) cmd = msg.Text.Substring(1);
                    else cmd = msg.Text.Substring(1, spaceIndex - 1);

                    var handler = Commands.FirstOrDefault(t => t.Command?.Equals(cmd, StringComparison.OrdinalIgnoreCase) ?? false);
                    if (handler != null)
                    {
                        await handler.ExecuteAsync(msg);
                        return;
                    }
                }

                await Commands.First(t => t.GetType() == typeof(NotFoundCommand)).ExecuteAsync(msg);
            }
        }

        private async Task SendMessageToOwnerAsync(string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false)
        {
            await _bot.SendTextMessageAsync(_ownerId, text, parseMode, disableWebPagePreview);
        }
    }
}

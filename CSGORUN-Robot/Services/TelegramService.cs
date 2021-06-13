using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Services.TelegramBotCommands;
using CSGORUN_Robot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telega;
using Telega.Client;
using Telega.Rpc.Dto.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services
{
    public class TelegramService : IAggregatorService
    {
        public bool IsActive => _client != null;
        public event EventHandler<IMessageWrapper> MessageReceived;

        private readonly ILogger<TelegramService> _log;
        private readonly TelegramBotService _botService;
        private readonly TgAggregatorCommand _commandInstance;

        private TelegramClient _client;

        public TelegramService(ILogger<TelegramService> logger)
        {
            _log = logger;

            _botService = Program.ServiceProvider.GetRequiredService<TelegramBotService>();
            _commandInstance = _botService.Commands.OfType<TgAggregatorCommand>().First();
        }

        public void Start()
        {
            var settings = AppSettingsProvider.Provide().Telegram.Aggregator;
            _client = TelegramClient.Connect(settings.ApiId).GetAwaiter().GetResult();

            EnsureAuthorizedAsync(_client).GetAwaiter().GetResult();
            EnsureChannelsJoinedAsync(_client, settings.Channels.Select(t => t.Username)).GetAwaiter().GetResult();
        }

        private async Task<string> BotReadLineAsync()
        {
            var resetEvent = new ManualResetEvent(false);
            string result = null;
            EventHandler<string> onCommandReceived = (s, e) =>
            {
                result = e;
                resetEvent.Set();
            };
            _commandInstance.CommandReceived += onCommandReceived;
            _log.LogInformation("[{0}] Awaiting a message", nameof(BotReadLineAsync));
            await Task.Run(() => resetEvent.WaitOne());
            _log.LogInformation("[{0}] Message received: '{1}'", nameof(BotReadLineAsync), result);
            _commandInstance.CommandReceived += onCommandReceived;
            return result;
        }

        private async Task EnsureAuthorizedAsync(TelegramClient client)
        {
            _log.LogInformation("[{0}] Ensuring client is authorized", nameof(EnsureAuthorizedAsync));
            if (!client.Auth.IsAuthorized)
            {
                var settings = AppSettingsProvider.Provide().Telegram.Aggregator;

                await _botService.SendMessageToOwnerAsync(
                    text: $"Telegram Aggregator requires the *phone number*.\n\nUse `/{_commandInstance.Command} <phone>` to enter it.",
                    parseMode: ParseMode.Markdown
                    );
                var phone = await BotReadLineAsync();

                var codeHash = await client.Auth.SendCode(settings.ApiHash, phone);

                await _botService.SendMessageToOwnerAsync(
                    text: $"Telegram Aggregator requires the *code you received*.\n\nUse `/{_commandInstance.Command} <code>` to enter it.",
                    parseMode: ParseMode.Markdown
                    );
                var code = await BotReadLineAsync();

                try
                {
                    await client.Auth.SignIn(phone, codeHash, code);
                }
                catch (TgPasswordNeededException)
                {
                    await _botService.SendMessageToOwnerAsync(
                        text: $"Telegram Aggregator requires the *2FA cloud password*.\n\nUse `/{_commandInstance.Command} <password> to enter it.",
                        parseMode: ParseMode.Markdown
                        );
                    var password = Console.ReadLine();
                    await client.Auth.CheckPassword(password);
                }

                await _botService.SendMessageToOwnerAsync(
                    text: $"Telegram Aggregator *login success*.",
                    parseMode: ParseMode.Markdown
                    );
            }

            _log.LogInformation("[{0}] Client authorized!", nameof(EnsureAuthorizedAsync));
        }

        private async Task EnsureChannelsJoinedAsync(TelegramClient client, IEnumerable<string> channelsUsernames)
        {
            _log.LogInformation("[{0}] Refreshing dialogs to ensure channels joined.", nameof(EnsureChannelsJoinedAsync));

            var dialogs = await _client.Messages.GetDialogs();
            var channelUsernames = dialogs.Default.Chats.Select(t => t?.Channel?.Username.FirstOrDefault()?.ToLower());
            var mustJoinUsernames = channelsUsernames.Select(t => t.TrimStart('@').ToLower());
            var needJoinUsernames = mustJoinUsernames.Except(channelUsernames);

            _log.LogInformation("[{0}] Need to join into {1} channels.", nameof(EnsureChannelsJoinedAsync), needJoinUsernames.Count());

            foreach (var username in needJoinUsernames)
            {
                _log.LogInformation("[0] Telegram Aggregator is joining to channel @{1}", nameof(EnsureChannelsJoinedAsync), username);

                var usernameResult = await client.Contacts.ResolveUsername(username);
                var channel = usernameResult.Chats[0].Channel;

                await client.Channels.JoinChannel(new InputChannel.DefaultTag(channel.Id, (long)channel.AccessHash));
            }
        }

        public void Stop()
        {
            _client.Dispose();
        }
    }
}

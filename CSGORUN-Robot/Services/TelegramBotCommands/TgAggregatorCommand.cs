using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class TgAggregatorCommand : TelegramBotCommandBase
    {
        public TgAggregatorCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "tgaggregator";
        public override string Description { get; set; } = "Input system directly to Telegram Aggregator Account";

        public event EventHandler<string> CommandReceived;

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (param?.Length > 1)
            {
                CommandReceived?.Invoke(this, param);
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Use `/{Command} <params>` to input params directly to Telegram Aggregator.",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );
            }
        }
    }
}

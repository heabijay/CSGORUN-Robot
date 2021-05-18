using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class NotFoundCommand : TelegramBotCommandBase
    {
        public NotFoundCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; }
        public override string Description { get; set; } = "Executes when input command was invalid";

        public override async Task ExecuteAsync(Message message)
        {
            await _bot.SendTextMessageAsync(
                chatId: message.From.Id,
                text: $"Sorry, input command _\"{message.Text}\"_ not found!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                disableWebPagePreview: true
                );
        }
    }
}

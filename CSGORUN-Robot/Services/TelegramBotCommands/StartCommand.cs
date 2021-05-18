using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class StartCommand : TelegramBotCommandBase
    {
        public StartCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "start";
        public override string Description { get; set; } = "Bot's welcome message!";

        public override async Task ExecuteAsync(Message message)
        {
            await _bot.SendTextMessageAsync(
                chatId: message.From.Id,
                text: $"Welcome! Use /help to get all commands!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                disableWebPagePreview: true
                );
        }
    }
}

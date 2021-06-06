using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class ShutdownCommand : TelegramBotCommandBase
    {
        public ShutdownCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
            GenerateCode();
        }

        public override string Command { get; set; } = "shutdown";
        public override string Description { get; set; } = "Performs shutdown of the CSGORUN-Robot.";

        private string _code;

        private readonly Random _random = new Random();
        private void GenerateCode()
        {
            _code = _random.Next(0, 100000).ToString("D5");
        }

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (param?.Length > 1)
            {
                if (param.Substring(1).Equals(_code, StringComparison.OrdinalIgnoreCase))
                {
                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Code correct! Performing shutdown... Bye-bye!",
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                    );
                    GenerateCode();
                    PerformShutdown();
                }
                else
                {
                    GenerateCode();
                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Code was incorrect!\n\nYour new code: *{_code}*.",
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                    );
                }
            }
            else
            {
                GenerateCode();
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Use `/{Command} <code>` to force shutdown the CSGORUN-Robot.\n\nYour code: *{_code}*",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );
            }
        }

        private void PerformShutdown()
        {
            Environment.Exit(0);
        }
    }
}

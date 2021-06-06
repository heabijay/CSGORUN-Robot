using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class DebugActivatePromoCommand : TelegramBotCommandBase
    {
        public DebugActivatePromoCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "debugactivatepromo";
        public override string Description { get; set; } = "Debug option. Performs activate promo at all accounts.";

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (param.Length > 1)
            {
                var promo = param.Trim();

                var worker = Program.ServiceProvider.GetRequiredService<Worker>();
                worker.EnqueuePromo(promo);

                await _bot.SendTextMessageAsync(
                    chatId: message.From.Id,
                    text: $"Promo `{promo}` has been enqueue!",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.From.Id,
                    text: $"Usage: `/{Command} <promo>`",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );
            }
        }
    }
}
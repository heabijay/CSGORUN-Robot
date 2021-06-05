using CSGORUN_Robot.CSGORUN.DTOs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class WithdrawsCommand : TelegramBotCommandBase
    {
        public WithdrawsCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "withdraws";
        public override string Description { get; set; } = "Calculates deposit / withdraws statistic.";

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);
            if (int.TryParse(param, out int id))
            {
                var infoMsg = await _bot.SendTextMessageAsync(
                    chatId: message.From.Id,
                    text: $"Calculating the deposits/withdraws statistic. Please wait!",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );

                var worker = Program.ServiceProvider.GetRequiredService<Worker>();
                try
                {
                    var client = worker.Clients[id];

                    var withdraws = await client.GetWithdrawsAsync();

                    await _bot.EditMessageTextAsync(
                        chatId: infoMsg.Chat.Id,
                        messageId: infoMsg.MessageId,
                        text: $"*{client.HttpService.LastCurrentState.user.name}'s withdrawals:*\n" +
                            $"Withdraw: _{Math.Round(withdraws.Where(t => t.status == WithdrawStatus.WITHDRAWN).Sum(t => t.amount), 2)}$_\n" +
                            $"Deposit: _{client.HttpService.LastCurrentState.user.deposit}$_",
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                        );
                }
                catch (Exception ex)
                {
                    await _bot.EditMessageTextAsync(
                        chatId: infoMsg.Chat.Id,
                        messageId: infoMsg.MessageId,
                        text: $"Exception: {ex.Message}\n\nStack Trace:\n{ex}",
                        parseMode: ParseMode.Default,
                        disableWebPagePreview: true
                        );
                }
            }
            else
            {
                await _bot.SendTextMessageAsync(
                    chatId: message.From.Id,
                    text: $"Usage: `/{Command} <accountId>`",
                    parseMode: ParseMode.Markdown,
                    disableWebPagePreview: true
                    );
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class DebugPlaceBetCommand : TelegramBotCommandBase
    {
        public DebugPlaceBetCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "debugplacebet";
        public override string Description { get; set; } = "Debug option. Places default bet.";

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (int.TryParse(param, out int id))
            {
                var worker = Program.ServiceProvider.GetRequiredService<Worker>();
                try
                {
                    var client = worker.Clients[id];

                    var infoMsg = await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                            $"PlaceBet operation: *Processing*",
                        parseMode: ParseMode.Markdown,
                        disableWebPagePreview: true
                    );

                    try
                    {
                        await client.PerformDefaultBetAsync();
                        await _bot.EditMessageTextAsync(
                            chatId: infoMsg.Chat.Id,
                            messageId: infoMsg.MessageId,
                            text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                $"PlaceBet operation: *Success*",
                            parseMode: ParseMode.Markdown,
                            disableWebPagePreview: true
                            );
                    }
                    catch (Exception ex)
                    {
                        await _bot.EditMessageTextAsync(
                            chatId: infoMsg.Chat.Id,
                            messageId: infoMsg.MessageId,
                            text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                $"PlaceBet operation: *Failed* ({ex.Message})",
                            parseMode: ParseMode.Markdown,
                            disableWebPagePreview: true
                            );
                    }
                }
                catch (Exception ex)
                {
                    await _bot.SendTextMessageAsync(
                        chatId: message.Chat.Id,
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
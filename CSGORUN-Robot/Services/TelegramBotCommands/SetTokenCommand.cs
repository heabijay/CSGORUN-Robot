using CSGORUN_Robot.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class SetTokenCommand : TelegramBotCommandBase
    {
        public SetTokenCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "settoken";
        public override string Description { get; set; } = "Sets a new token to selected account.";

        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (param?.Length > 1)
            {
                var @params = param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (@params.Length == 2 && int.TryParse(@params[0], out int id))
                {
                    var worker = Program.ServiceProvider.GetRequiredService<Worker>();
                    try
                    {
                        var client = worker.Clients[id];

                        client.Account.AuthToken = @params[1];
                        var infoMsg = await _bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                $"Token: `*****{client.Account.AuthToken.Substring(client.Account.AuthToken.Length - 7)}`\n" +
                                $"Status: *Verifing*",
                            parseMode: ParseMode.Markdown,
                            disableWebPagePreview: true
                            );
                        try
                        {
                            await client.HttpService.GetCurrentStateAsync();
                            var aggregator = worker.Aggregators.Find(t => t.GetType() == typeof(CsgorunService));
                            if (!aggregator.IsActive)
                                aggregator.Start();

                            await _bot.EditMessageTextAsync(
                                chatId: infoMsg.Chat.Id,
                                messageId: infoMsg.MessageId,
                                text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                    $"Token: `*****{client.Account.AuthToken.Substring(client.Account.AuthToken.Length - 7)}`\n" +
                                    $"Status: *Success*",
                                parseMode: ParseMode.Markdown,
                                disableWebPagePreview: true
                                );
                        }
                        catch (HttpRequestRawException ex)
                        {
                            await _bot.EditMessageTextAsync(
                                chatId: infoMsg.Chat.Id,
                                messageId: infoMsg.MessageId,
                                text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                    $"Token: `*****{client.Account.AuthToken.Substring(client.Account.AuthToken.Length - 7)}`\n" +
                                    $"Status: *Failed* ({(int)ex.InnerException.StatusCode})",
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
                    return;
                }
            }

            await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Use `/{Command} <accountId> <token>` to apply.",
                parseMode: ParseMode.Markdown,
                disableWebPagePreview: true
                );
        }
    }
}

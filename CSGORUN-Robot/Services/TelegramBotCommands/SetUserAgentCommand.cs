using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class SetUserAgentCommand : TelegramBotCommandBase
    {
        public SetUserAgentCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "setuseragent";
        public override string Description { get; set; } = "Sets a new user-agent to selected account.";
        public override async Task ExecuteAsync(Message message)
        {
            var param = message.Text.Substring(Command.Length + 1);

            if (param?.Length > 1)
            {
                var @params = param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (@params.Length > 2 && int.TryParse(@params[0], out int id))
                {
                    var worker = Program.ServiceProvider.GetRequiredService<Worker>();
                    try
                    {
                        var client = worker.Clients[id];

                        client.Account.UserAgent = param.Substring(param.IndexOf(@params[1], StringComparison.Ordinal)).Trim();
                        var infoMsg = await _bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"*#{id} - {client.HttpService.LastCurrentState.user.name}*\n" +
                                $"User-Agent: `{client.Account.UserAgent}`\n" + 
                                $"Successfully set!",
                            parseMode: ParseMode.Markdown,
                            disableWebPagePreview: true
                            );
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
                text: $"Use `/{Command} <accountId> <userAgent>` to apply.",
                parseMode: ParseMode.Markdown,
                disableWebPagePreview: true
                );
        }
    }
}
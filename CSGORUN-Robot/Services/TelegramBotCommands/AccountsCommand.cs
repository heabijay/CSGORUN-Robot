using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class AccountsCommand : TelegramBotCommandBase
    {
        public AccountsCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "accounts";
        public override string Description { get; set; } = "Provides latest information about accounts.";

        public override async Task ExecuteAsync(Message message)
        {
            var worker = Program.ServiceProvider.GetRequiredService<Worker>();

            var sb = new StringBuilder();
            sb.AppendLine("*Accounts:* ");

            int i = 0;
            foreach (var client in worker.Clients)
            {
                var state = client.HttpService.LastCurrentState?.user;
                sb.AppendLine($"#{i}: _{state.name} ({state.balance}$)_");
                sb.AppendLine($"[CSGORUN]({CSGORUN.Routing.HomeProfileEndpoint}/{state.steamId}) | [Steam](https://steamcommunity.com/profiles/{state.steamId})");
                sb.AppendLine($"Token: `*****{client.Account.AuthToken.Substring(client.Account.AuthToken.Length - 7)}` *({(client.HttpService.IsAuthorized ? "Authorized" : "Unauthorized")})*");
                sb.AppendLine($"User-Agent: `{client.Account.UserAgent}`");
                sb.AppendLine($"Proxy: `{client.Account.Proxy?.Host ?? "null"}`");
                if (state.items?.Count > 0)
                    foreach (var item in state.items)
                        sb.AppendLine($"_> {item.name} ({item.price}$)_");

                sb.AppendLine();
                i++;
            }

            await _bot.SendTextMessageAsync(
                chatId: message.From.Id,
                text: sb.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                disableWebPagePreview: true
                );
        }
    }
}

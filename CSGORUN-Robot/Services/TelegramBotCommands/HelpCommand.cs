using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public class HelpCommand : TelegramBotCommandBase
    {
        public HelpCommand(TelegramBotClient bot, TelegramBotService botService) : base(bot, botService)
        {
        }

        public override string Command { get; set; } = "help";
        public override string Description { get; set; } = "Provides the help information with available commands.";

        public override async Task ExecuteAsync(Message message)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Hi! I'm CSGORUN-Robot. I know the next commands: ");
            foreach (var cmd in _botService.Commands)
            {
                if (cmd.Command != null)
                {
                    var cmdStr = "_";
                    cmdStr += "/" + cmd.Command;

                    if (cmd.Description != null) cmdStr += " — " + cmd.Description;

                    cmdStr += "_";

                    sb.AppendLine(cmdStr);
                }
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

using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public interface ITelegramBotCommand
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public void Execute(Message message);
        public Task ExecuteAsync(Message message);
    }
}

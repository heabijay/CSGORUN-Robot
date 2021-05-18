using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CSGORUN_Robot.Services.TelegramBotCommands
{
    public abstract class TelegramBotCommandBase : ITelegramBotCommand
    {
        protected TelegramBotClient _bot;
        protected TelegramBotService _botService;

        protected TelegramBotCommandBase(TelegramBotClient bot, TelegramBotService botService)
        {
            _bot = bot;
            _botService = botService;
        }

        public abstract string Command { get; set; }
        public abstract string Description { get; set; }

        public virtual void Execute(Message message)
        {
            ExecuteAsync(message).GetAwaiter().GetResult();
        }

        public abstract Task ExecuteAsync(Message message);
    }
}
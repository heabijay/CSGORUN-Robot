using System.Threading.Tasks;

namespace CSGORUN_Robot.Client.GameControllers
{
    public abstract class GameControllerBase : IGameController
    {
        protected ClientWorker _client;
        public GameControllerBase(ClientWorker client)
        {
            _client = client;
        }
        public abstract Task AwaitGameStartAsync();
        public abstract Task MakeQuickBetAsync(int itemId);
    }
}

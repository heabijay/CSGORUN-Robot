using System.Threading.Tasks;

namespace CSGORUN_Robot.Client.GameControllers
{
    public class CrashGameController : GameControllerBase
    {
        public CrashGameController(ClientWorker client) : base(client)
        {
        }

        public override Task AwaitGameStartAsync() => _client.AwaitCrashGameStartAsync();
        public override Task MakeQuickBetAsync(int itemId) => _client.HttpService.PostCrashMakeBetAsync(itemId, 1.01);
    }
}

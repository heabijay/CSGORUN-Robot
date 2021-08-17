using System.Threading.Tasks;

namespace CSGORUN_Robot.Client.GameControllers
{
    public interface IGameController
    {
        public Task MakeQuickBetAsync(int itemId);
        public Task AwaitGameStartAsync();
    }
}

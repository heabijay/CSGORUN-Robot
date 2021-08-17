using CSGORUN_Robot.CSGORUN.DTOs;
using System;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Client.GameControllers
{
    public class RouletteGameController : GameControllerBase
    {
        protected static Random _random = new Random();

        public RouletteGameController(ClientWorker client) : base(client)
        {
        }

        public override Task AwaitGameStartAsync() => _client.AwaitRouletteGameStartAsync();
        public override Task MakeQuickBetAsync(int itemId) => _client.HttpService.PostRouletteMakeBetAsync(itemId, (RouletteColor)_random.Next(1, 3));
    }
}

using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Movement.Actions
{
    public class StanceAction : IMovementAction
    {
        public StanceAction(EPlayerStance ePlayerStance)
        {
            Stance = ePlayerStance;
        }

        public EPlayerStance Stance { get; }

        public async Task Do(PlayerDummy dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Data.UnturnedUser.Player.Player.stance.stance = Stance;
        }
    }
}

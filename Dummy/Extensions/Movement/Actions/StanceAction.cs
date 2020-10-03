using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Extensions.Movement.Actions
{
    public class StanceAction : IMovementAction
    {
        public StanceAction(EPlayerStance ePlayerStance)
        {
            Stance = ePlayerStance;
        }

        public EPlayerStance Stance { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Player.Player.stance.stance = Stance;
        }
    }
}

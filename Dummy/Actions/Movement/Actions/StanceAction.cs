using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;

namespace Dummy.Actions.Movement.Actions
{
    public class StanceAction : IAction
    {
        public StanceAction(EPlayerStance ePlayerStance)
        {
            Stance = ePlayerStance;
        }

        public EPlayerStance Stance { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask SetStance()
            {
                await UniTask.SwitchToMainThread();
                dummy.Player.Player.stance.stance = Stance;
            }

            return SetStance().AsTask();
        }
    }
}
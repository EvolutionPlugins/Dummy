using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Actions.Movement.Actions
{
    public class JumpAction : IAction
    {
        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;
            async UniTask Jump()
            {
                await UniTask.SwitchToMainThread();
                byte analog = (byte)(player.movement.horizontal << 4 | player.movement.vertical);
                player.movement.simulate(1u, 1, (analog >> 4 & 15) - 1, (analog & 15) - 1, 0f, 0f, true, false,
                    player.transform.localPosition, PlayerInput.RATE);
            }
            return Jump().AsTask();
        }
    }
}
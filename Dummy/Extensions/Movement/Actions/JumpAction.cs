using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Movement.Actions
{
    public class JumpAction : IMovementAction
    {
        public Task Do(PlayerDummy dummy)
        {
            Player player = dummy.Data.UnturnedUser.Player.Player;
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
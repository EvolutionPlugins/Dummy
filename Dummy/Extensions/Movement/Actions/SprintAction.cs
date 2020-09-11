using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy;
using EvolutionPlugins.Dummy.Extensions.Movement;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Extensions.Movement.Actions
{
    public class SprintAction : IMovementAction
    {
        public Task Do(PlayerDummy dummy)
        {
            var player = dummy.Data.UnturnedUser.Player;
            async UniTask Sprint()
            {
                await UniTask.SwitchToMainThread();
                player.Player.movement.simulate(1, 0, player.Player.movement.horizontal - 1, player.Player.movement.vertical - 1,
                player.Player.look.look_x, player.Player.look.look_y, false, true, Vector3.zero, PlayerInput.RATE);
            }
            return Sprint().AsTask();
        }
    }
}

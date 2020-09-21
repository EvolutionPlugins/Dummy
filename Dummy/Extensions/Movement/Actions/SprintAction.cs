using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Extensions.Movement.Actions
{
    public class SprintAction : IMovementAction
    {
        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;
            async UniTask Sprint()
            {
                await UniTask.SwitchToMainThread();
                player.movement.simulate(1, 0, player.movement.horizontal - 1, player.movement.vertical - 1,
                player.look.look_x, player.look.look_y, false, true, Vector3.zero, PlayerInput.RATE);
            }
            return Sprint().AsTask();
        }
    }
}

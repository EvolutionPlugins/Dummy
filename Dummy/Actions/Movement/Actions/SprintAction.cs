using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    public class SprintAction : IAction
    {
        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;
            async UniTask Sprint()
            {
                await UniTask.SwitchToMainThread();

                player.movement.simulate(dummy.Simulation.Simulation, dummy.Simulation.Recov, player.movement.horizontal - 1, player.movement.vertical - 1,
                    player.look.look_x, player.look.look_y, false, true, Vector3.zero, PlayerInput.RATE, false);
            }
            return Sprint().AsTask();
        }
    }
}

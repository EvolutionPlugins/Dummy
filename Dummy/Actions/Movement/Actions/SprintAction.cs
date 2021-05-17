extern alias JetBrainsAnnotations;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    [UsedImplicitly]
    public class SprintAction : IAction
    {
        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;

            async UniTask Sprint()
            {
                await UniTask.SwitchToMainThread();
                
                // FIXME

                // player.movement.simulate(dummy.Simulation.Simulation, dummy.Simulation.Recov,
                //     player.movement.horizontal - 1, player.movement.vertical - 1,
                //     player.look.look_x, player.look.look_y, false, true, Vector3.zero,
                //     PlayerInput.RATE, false);
            }

            return Sprint().AsTask();
        }
    }
}
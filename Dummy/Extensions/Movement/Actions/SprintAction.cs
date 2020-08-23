using EvolutionPlugins.Dummy;
using EvolutionPlugins.Dummy.Extensions.Movement;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Extensions.Movement.Actions
{
    public class SprintAction : IMovementAction
    {
        public void Do(PlayerDummy dummy)
        {
            var player = dummy.Data.UnturnedUser.Player;
            player.movement.simulate(1, 0, player.movement.horizontal - 1, player.movement.vertical - 1,
                player.look.look_x, player.look.look_y, false, true, Vector3.zero, PlayerInput.RATE);
        }
    }
}

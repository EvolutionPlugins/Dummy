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
            player.Player.movement.simulate(1, 0, player.Player.movement.horizontal - 1, player.Player.movement.vertical - 1,
                player.Player.look.look_x, player.Player.look.look_y, false, true, Vector3.zero, PlayerInput.RATE);
        }
    }
}

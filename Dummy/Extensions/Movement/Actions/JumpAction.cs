using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public class JumpAction : IMovementAction
    {
        public void Do(PlayerDummy dummy)
        {
            Player player = dummy.Data.UnturnedUser.Player.Player;
            byte analog = (byte) (player.movement.horizontal << 4 | player.movement.vertical);
            player.movement.simulate(1u, 1, (analog >> 4 & 15) - 1, (analog & 15) - 1, 0f, 0f, true, false,
                player.transform.localPosition, PlayerInput.RATE);
        }
    }
}
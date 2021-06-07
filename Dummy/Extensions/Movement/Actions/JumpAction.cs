using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public class JumpAction : IMovementAction
    {
        private readonly bool _sprint;

        public JumpAction(bool sprint = false)
        {
            _sprint = sprint;
        }
        
        public void Do(PlayerDummy dummy)
        {
            Player player = dummy.Data.UnturnedUser.Player.Player;
            byte analog = (byte) (player.movement.horizontal << 4 | player.movement.vertical);
            player.movement.simulate(player.input.simulation, player.input.recov, (analog >> 4 & 15) - 1, (analog & 15) - 1, 0f, 0f, true, _sprint,
                player.transform.localPosition, PlayerInput.RATE, false);
        }
    }
}
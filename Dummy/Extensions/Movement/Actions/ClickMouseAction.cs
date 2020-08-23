using EvolutionPlugins.Dummy;
using EvolutionPlugins.Dummy.API;

namespace Dummy.Extensions.Movement.Actions
{
    public class ClickMouseAction : IAction
    {
        public ClickMouseAction(ClickMouseState state)
        {
            State = state;
        }

        public ClickMouseState State { get; }

        public void Do(PlayerDummy dummy)
        {
            var player = dummy.Data.UnturnedUser.Player;
            if (State == ClickMouseState.LeftClick)
                player.equipment.simulate(1, false, true, false);
            else
                player.equipment.simulate(1, true, false, false);
        }
    }
}

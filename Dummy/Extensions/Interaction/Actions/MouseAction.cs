using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
{
    public class MouseAction : IInteractionAction
    {
        public MouseAction(MouseState state)
        {
            State = state;
        }

        public MouseState State { get; }

        public Task Do(PlayerDummy dummy)
        {
            var player = dummy.Data.UnturnedUser.Player;

            async UniTask ClickMouse()
            {
                await UniTask.SwitchToMainThread();
                if (State == MouseState.Left)
                    player.Player.equipment.simulate(player.Player.input.simulation, false, true, false);
                else
                    player.Player.equipment.simulate(player.Player.input.simulation, true, false, false);
            }
            return ClickMouse().AsTask();
        }
    }
}

using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
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

        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;

            async UniTask ClickMouse()
            {
                await UniTask.SwitchToMainThread();
                if (State == MouseState.Left)
                    player.equipment.simulate(player.input.simulation, false, true, false);
                else
                    player.equipment.simulate(player.input.simulation, true, false, false);
            }
            return ClickMouse().AsTask();
        }
    }
}

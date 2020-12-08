using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction;
using Dummy.API;
using Dummy.Users;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    public class MouseAction : IAction
    {
        public MouseAction(MouseState state)
        {
            State = state;
        }

        public MouseState State { get; }

        public Task Do(DummyUser dummy)
        {
            var player = dummy.Player.Player;

            // rewrite to use DummyUserSimulationThread
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

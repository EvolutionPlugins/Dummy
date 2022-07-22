extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    [UsedImplicitly]
    public class MouseAction : IAction
    {
        public MouseAction(MouseState state)
        {
            State = state;
        }

        public MouseState State { get; }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.MouseState = State;
            return Task.CompletedTask;
        }
    }
}
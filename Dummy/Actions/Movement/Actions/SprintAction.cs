extern alias JetBrainsAnnotations;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions.Movement.Actions
{
    [UsedImplicitly]
    public class SprintAction : IAction
    {
        public bool IsSprinting { get; }

        public SprintAction(bool isSprinting)
        {
            IsSprinting = isSprinting;
        }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.Sprint = IsSprinting;
            return Task.CompletedTask;
        }
    }
}
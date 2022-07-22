using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;

namespace Dummy.Actions.Movement.Actions
{
    public class JumpAction : IAction
    {
        public bool IsJumping { get; }

        public JumpAction(bool isJumping)
        {
            IsJumping = isJumping;
        }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.Jump = IsJumping;
            return Task.CompletedTask;
        }
    }
}
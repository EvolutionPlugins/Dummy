using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;

namespace Dummy.Actions.Movement.Actions
{
    public class StanceAction : IAction
    {
        public StanceAction(bool wantsCrouch, bool wantsProne)
        {
            WantsCrouch = wantsCrouch;
            WantsProne = wantsProne;
        }

        public bool WantsCrouch { get; }
        public bool WantsProne { get; }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.Crouch = WantsCrouch;
            dummy.Simulation.Prone = WantsProne;

            return Task.CompletedTask;
        }
    }
}
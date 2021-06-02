using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;

namespace Dummy.Actions.Movement.Actions
{
    public class StanceAction : IAction
    {
        public StanceAction(bool wantsCrouch, bool wantsProne)
        {
            WantsCrouch = wantsCrouch;
            WantsProne = wantsProne;
        }

        public StanceAction(EPlayerStance stance)
        {
            WantsCrouch = stance == EPlayerStance.CROUCH;
            WantsProne = stance == EPlayerStance.PRONE;
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
using System;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    public class StrafeAction : IAction
    {
        public StrafeDirection Dir { get; }

        public StrafeAction(StrafeDirection dir)
        {
            Dir = dir;
        }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.Move = Dir switch
            {
                StrafeDirection.None => new Vector2(0, 0),
                StrafeDirection.Left => new(-1, 0),
                StrafeDirection.Right => new(1, 0),
                StrafeDirection.Forward => new(0, 1),
                StrafeDirection.Backward => new(0, -1),
                StrafeDirection.ForwardLeft => new(-1, 1),
                StrafeDirection.ForwardRight => new(1, 1),
                StrafeDirection.BackwardLeft => new(-1, -1),
                StrafeDirection.BackwardRight => new(1, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(Dir), Dir, "Tried to strafe to wrong direction")
            };
            return Task.CompletedTask;
        }
    }
}
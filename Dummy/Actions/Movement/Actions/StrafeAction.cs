using System;
using System.Reflection;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
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
            var move = Dir switch
            {
                StrafeDirection.Left => new Vector3(-1, 0),
                StrafeDirection.Right => new(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(Dir), Dir, "Tried to strafe to wrong direction")
            };
            
            dummy.Simulation.Move = move;
            return Task.CompletedTask;
        }
    }
}
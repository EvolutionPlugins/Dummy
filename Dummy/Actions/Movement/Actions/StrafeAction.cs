using System;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    public class StrafeAction : IAction
    {
        private Vector3? m_Vector;

        public StrafeDirection Direction { get; }

        public StrafeAction(StrafeDirection direction)
        {
            Direction = direction;
        }

        public StrafeAction(int x, int y) : this(new Vector2(x, y))
        {
        }

        public StrafeAction(Vector2 vector)
        {
            m_Vector = new(vector.x, 0, vector.y);
        }

        public Task Do(DummyUser dummy)
        {
            dummy.Simulation.Move = m_Vector ?? Direction switch
            {
                StrafeDirection.None => new Vector3(0, 0, 0),
                StrafeDirection.Left => new(-1, 0, 0),
                StrafeDirection.Right => new(1, 0, 0),
                StrafeDirection.Forward => new(0, 0, 1),
                StrafeDirection.Backward => new(0, 0, -1),
                StrafeDirection.ForwardLeft => new(-1, 0, 1),
                StrafeDirection.ForwardRight => new(1, 0, 1),
                StrafeDirection.BackwardLeft => new(-1, 0, -1),
                StrafeDirection.BackwardRight => new(1, 0, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, "Tried to strafe to wrong direction")
            };
            return Task.CompletedTask;
        }
    }
}
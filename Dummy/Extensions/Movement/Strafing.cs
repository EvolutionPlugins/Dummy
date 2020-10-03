using Dummy.Extensions.Movement.Actions;
using Dummy.Users;

namespace Dummy.Extensions.Movement
{
    public static class Strafing
    {
        public static void WalkingConstantOn(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.Add(new StrafeAction(direction));
        }

        public static void WalkingConstantOff(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c => c is StrafeAction strafeAction && strafeAction.Dir == direction);
        }

        public static void Walk(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.Actions.Enqueue(new StrafeAction(direction));
        }
    }
}
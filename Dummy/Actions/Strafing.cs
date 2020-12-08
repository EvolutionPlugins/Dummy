using Dummy.Actions.Movement.Actions;
using Dummy.Users;

namespace Dummy.Actions
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
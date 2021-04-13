extern alias JetBrainsAnnotations;
using Dummy.Actions.Movement.Actions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly]
    public static class Strafing
    {
        [UsedImplicitly]
        public static void WalkingConstantOn(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.Add(new StrafeAction(direction));
        }

        [UsedImplicitly]
        public static void WalkingConstantOff(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c =>
                c is StrafeAction strafeAction && strafeAction.Dir == direction);
        }

        [UsedImplicitly]
        public static void Walk(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.Actions.Enqueue(new StrafeAction(direction));
        }
    }
}
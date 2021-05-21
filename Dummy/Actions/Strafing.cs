extern alias JetBrainsAnnotations;
using Dummy.Actions.Movement.Actions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public static class Strafing
    {
        public static void WalkingConstantOn(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.Add(new StrafeAction(direction));
        }
        
        public static void WalkingConstantOff(this DummyUser dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c =>
                c is StrafeAction strafeAction && strafeAction.Dir == direction);
        }
        
        public static void Walk(this DummyUser dummy, StrafeDirection direction)
        {
            Sprint(dummy, direction, false);
        }

        public static void Sprint(this DummyUser dummyUser, StrafeDirection direction, bool isSprinting)
        {
            dummyUser.Actions.Actions.Enqueue(new StrafeAction(direction));
            dummyUser.Actions.Actions.Enqueue(new SprintAction(isSprinting));
        }
    }
}
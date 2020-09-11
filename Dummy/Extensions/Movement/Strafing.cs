using System.Linq;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public static class Strafing
    {
        public static void WalkingConstantOn(this PlayerDummy dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.Add(new StrafeAction(direction));
        }

        public static void WalkingConstantOff(this PlayerDummy dummy, StrafeDirection direction)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c => c is StrafeAction strafeAction && strafeAction.Dir == direction);
        }

        public static void Walk(this PlayerDummy dummy, StrafeDirection direction)
        {
            dummy.Actions.Actions.Enqueue(new StrafeAction(direction));
        }
    }
}
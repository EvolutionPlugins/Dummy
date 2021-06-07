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
            dummy.Actions.ContinuousActions.Remove(dummy.Actions.ContinuousActions
                .FirstOrDefault(c => c is StrafeAction action && action.Dir == direction));
        }

        public static void Walk(this PlayerDummy dummy, StrafeDirection direction)
        {
            new StrafeAction(direction).Do(dummy);
        }
    }
}
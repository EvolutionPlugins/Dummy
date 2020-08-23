namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public static class Jumping
    {
        public static void JumpingConstantOn(this PlayerDummy dummy)
        {
            dummy.Actions.ContinuousActions.Add(new JumpAction());
        }

        public static void JumpingConstantOff(this PlayerDummy dummy)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c => c is JumpAction);
        }

        public static void Jump(this PlayerDummy dummy)
        {
            new JumpAction().Do(dummy);
        }
    }
}
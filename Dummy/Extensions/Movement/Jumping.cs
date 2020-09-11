using System.Threading.Tasks;

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
            dummy.Actions.Actions.Enqueue(new JumpAction());
        }

        public static async Task TempJump(this PlayerDummy playerDummy, float time)
        {
            playerDummy.JumpingConstantOn();
            await Task.Delay((int)(time * 1000));
            playerDummy.JumpingConstantOff();
        }
    }
}
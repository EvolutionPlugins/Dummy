using Dummy.Extensions.Movement.Actions;
using Dummy.Users;
using System.Threading.Tasks;

namespace Dummy.Extensions.Movement
{
    public static class Jumping
    {
        public static void JumpingConstantOn(this DummyUser dummy)
        {
            dummy.Actions.ContinuousActions.Add(new JumpAction());
        }

        public static void JumpingConstantOff(this DummyUser dummy)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c => c is JumpAction);
        }

        public static void Jump(this DummyUser dummy)
        {
            dummy.Actions.Actions.Enqueue(new JumpAction());
        }

        public static async Task TempJump(this DummyUser playerDummy, float time)
        {
            playerDummy.JumpingConstantOn();
            await Task.Delay((int)(time * 1000));
            playerDummy.JumpingConstantOff();
        }
    }
}
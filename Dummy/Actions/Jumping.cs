extern alias JetBrainsAnnotations;
using Dummy.Actions.Movement.Actions;
using Dummy.Users;
using System;
using System.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly]
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

        [UsedImplicitly]
        public static void Jump(this DummyUser dummy)
        {
            dummy.Actions.Actions.Enqueue(new JumpAction());
        }

        [UsedImplicitly]
        public static async Task TempJump(this DummyUser playerDummy, float time)
        {
            playerDummy.JumpingConstantOn();
            await Task.Delay(TimeSpan.FromSeconds(time));
            playerDummy.JumpingConstantOff();
        }
    }
}
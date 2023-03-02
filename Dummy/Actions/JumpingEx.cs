extern alias JetBrainsAnnotations;
using System;
using System.Threading.Tasks;
using Dummy.Actions.Movement.Actions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public static class JumpingEx
    {
        public static void JumpingConstantOn(this DummyUser dummy)
        {
            dummy.Actions.ContinuousActions.Add(new JumpAction(true));
        }

        public static void JumpingConstantOff(this DummyUser dummy)
        {
            dummy.Actions.ContinuousActions.RemoveAll(c => c is JumpAction);
        }

        public static void Jump(this DummyUser dummy)
        {
            dummy.Actions.Actions.Enqueue(new JumpAction(true));
            dummy.Actions.Actions.Enqueue(new JumpAction(false));
        }

        public static async Task TempJumpAsync(this DummyUser playerDummy, float time)
        {
            playerDummy.JumpingConstantOn();
            await Task.Delay(TimeSpan.FromSeconds(time));
            playerDummy.JumpingConstantOff();
        }
    }
}
using Dummy.API;
using Dummy.Users;
using HarmonyLib;
using SDG.Unturned;
using System.Reflection;
using System.Threading.Tasks;

namespace Dummy.Actions.Movement.Actions
{
    public class JumpAction : IAction
    {
        private const float JUMP = 7;
        private readonly static MethodInfo FallSetter = AccessTools.PropertySetter(typeof(PlayerMovement), "fall");

        public Task Do(DummyUser dummy)
        {
            var movement = dummy.Player.Player.movement;
            FallSetter.Invoke(movement, new[] { (object)(JUMP * (1f + (movement.player.skills.mastery(0, 6) * 0.25f)) * movement.pluginJumpMultiplier) });
            return Task.CompletedTask;
        }
    }
}
using System.Reflection;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using HarmonyLib;
using SDG.Unturned;

namespace Dummy.Actions.Movement.Actions
{
    public class JumpAction : IAction
    {
        private const float c_Jump = 7;
        private static readonly MethodInfo s_СFallSetter = AccessTools.PropertySetter(typeof(PlayerMovement), "fall");

        public Task Do(DummyUser dummy)
        {
            var movement = dummy.Player.Player.movement;
            s_СFallSetter.Invoke(movement,
                new[]
                {
                    (object)(c_Jump * (1f + (movement.player.skills.mastery(0, 6) * 0.25f)) *
                             movement.pluginJumpMultiplier)
                });
            return Task.CompletedTask;
        }
    }
}
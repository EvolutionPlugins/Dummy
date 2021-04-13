using System.Reflection;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;

namespace Dummy.Actions.Movement.Actions
{
    public class StrafeAction : IAction
    {
        private static readonly FieldInfo? s_HorizontalField;
        public StrafeDirection Dir { get; }

        static StrafeAction()
        {
            s_HorizontalField = typeof(PlayerMovement).GetField("_horizontal");
        }

        public StrafeAction(StrafeDirection dir)
        {
            Dir = dir;
        }

        public Task Do(DummyUser dummy)
        {
            //TODO: Use harmony to manually add a setter for the public property
            var offset = Dir == StrafeDirection.Left ? 1 : -1;
            s_HorizontalField?.SetValue(dummy.Player.Player.movement, dummy.Player.Player.movement.horizontal + offset);
            return Task.CompletedTask;
        }
    }
}
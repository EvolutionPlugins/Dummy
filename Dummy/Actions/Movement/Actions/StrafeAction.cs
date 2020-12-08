using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Reflection;
using System.Threading.Tasks;

namespace Dummy.Actions.Movement.Actions
{
    public class StrafeAction : IAction
    {
        private static readonly FieldInfo m_HorizontalField;
        public StrafeDirection Dir { get; }

        static StrafeAction()
        {
            m_HorizontalField = typeof(PlayerMovement).GetField("_horizontal");
        }

        public StrafeAction(StrafeDirection dir)
        {
            Dir = dir;
        }

        public Task Do(DummyUser dummy)
        {
            //TODO: Use harmony to manually add a setter for the public property
            int offset;
            if (Dir == StrafeDirection.Left) offset = 1;
            else offset = -1;
            m_HorizontalField?.SetValue(dummy.Player.Player.movement, dummy.Player.Player.movement.horizontal + offset);
            return Task.CompletedTask;
        }
    }
}
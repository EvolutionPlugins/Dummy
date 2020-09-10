using SDG.Unturned;
using System.Reflection;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public class StrafeAction : IMovementAction
    {
        private static readonly FieldInfo m_HorizontalField = InitFieldInfo();
        public StrafeDirection Dir { get; }

        private static FieldInfo InitFieldInfo()
        {
            return typeof(PlayerMovement).GetField("_horizontal");
        }

        public StrafeAction(StrafeDirection dir)
        {
            Dir = dir;
        }

        public void Do(PlayerDummy dummy)
        {
            //TODO: Use harmony to manually add a setter for the public property
            int offset;
            if (Dir == StrafeDirection.Left) offset = 1;
            else offset = -1;
            m_HorizontalField?.SetValue(dummy.Data.UnturnedUser.Player.Player.movement, dummy.Data.UnturnedUser.Player.Player.movement.horizontal + offset);
        }
    }
}
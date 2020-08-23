using SDG.Unturned;
using System.Reflection;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public class StrafeAction : IMovementAction
    {
        private readonly FieldInfo m_HorizontalField;
        public StrafeDirection Dir { get; }

        public StrafeAction(StrafeDirection dir)
        {
            Dir = dir;
            m_HorizontalField = typeof(PlayerMovement).GetField("_horizontal");
        }

        public void Do(PlayerDummy dummy)
        {
            //TODO: Use harmony to manually add a setter for the public property
            int offset;
            if (Dir == StrafeDirection.Left) offset = 1;
            else offset = -1;
            m_HorizontalField?.SetValue(dummy.Data.UnturnedUser.Player.movement, dummy.Data.UnturnedUser.Player.movement.horizontal + offset);
        }
    }
}
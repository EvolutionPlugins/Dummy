using EvolutionPlugins.Dummy.Models;
using SDG.Unturned;
using System.Reflection;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Movement.Actions
{
    public class StrafeAction : IMovementAction
    {
        private readonly FieldInfo m_HorizontalField;
        public StrafeDirection Dir { get; }

        public StrafeAction(StrafeDirection dir)
        {
            m_HorizontalField = typeof(PlayerMovement).GetField("_horizontal");
            Dir = dir;
        }

        public Task Do(PlayerDummy dummy)
        {
            //TODO: Use harmony to manually add a setter for the public property
            int offset;
            if (Dir == StrafeDirection.Left) offset = 1;
            else offset = -1;
            m_HorizontalField?.SetValue(dummy.Data.UnturnedUser.Player.Player.movement, dummy.Data.UnturnedUser.Player.Player.movement.horizontal + offset);
            return Task.CompletedTask;
        }
    }
}
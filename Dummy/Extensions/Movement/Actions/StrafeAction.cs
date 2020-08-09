using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Extensions.Movement
{
    public class StrafeAction : IMovementAction
    {

        public StrafeDirection Dir;
        
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
            typeof(PlayerMovement).GetField("_horizontal")?.SetValue(dummy.Data.UnturnedUser.Player.movement, dummy.Data.UnturnedUser.Player.movement.horizontal + offset);
        }
    }
}
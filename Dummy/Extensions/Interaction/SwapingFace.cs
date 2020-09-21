using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using EvolutionPlugins.Dummy.Models.Users;

namespace EvolutionPlugins.Dummy.Extensions.Interaction
{
    public static class SwapingFace
    {
        public static void SwapFace(this DummyUser dummy, byte index)
        {
            dummy.Actions.Actions.Enqueue(new FaceAction(index));
        }
    }
}

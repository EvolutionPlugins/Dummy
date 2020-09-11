using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;

namespace EvolutionPlugins.Dummy.Extensions.Interaction
{
    public static class SwapingFace
    {
        public static void SwapFace(this PlayerDummy dummy, byte index)
        {
            dummy.Actions.Actions.Enqueue(new FaceAction(index));
        }
    }
}

extern alias JetBrainsAnnotations;
using Dummy.Actions.Interaction.Actions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public static class SwapingFace
    {
        public static void SwapFace(this DummyUser dummy, byte index)
        {
            dummy.Actions.Actions.Enqueue(new FaceAction(index));
        }
    }
}
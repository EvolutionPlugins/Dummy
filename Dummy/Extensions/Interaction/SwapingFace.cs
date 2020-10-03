using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;

namespace Dummy.Extensions.Interaction
{
    public static class SwapingFace
    {
        public static void SwapFace(this DummyUser dummy, byte index)
        {
            dummy.Actions.Actions.Enqueue(new FaceAction(index));
        }
    }
}

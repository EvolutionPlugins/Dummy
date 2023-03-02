using Dummy.Actions.Interaction;
using Dummy.Actions.Interaction.Actions;
using Dummy.Users;

namespace Dummy.Actions;
public static class MouseEx
{
    public static void Punch(this DummyUser dummy, MouseState state)
    {
        dummy.Actions.Actions.Enqueue(new MouseAction(state));
        dummy.Actions.Actions.Enqueue(new MouseAction(MouseState.None));
    }
}

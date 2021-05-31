using System;

namespace Dummy.Actions.Interaction
{
    [Flags]
    public enum MouseState
    {
        None = 0,
        Left = 1,
        Right = 1 << 1,
        LeftRight = Left | Right
    }
}

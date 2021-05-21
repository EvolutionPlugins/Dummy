namespace Dummy.Actions
{
    public enum StrafeDirection
    {
        None = 0,
        Left = 1,
        Right = 1 << 1,
        Forward = 1 << 2,
        Backward = 1 << 3,
        ForwardLeft = Forward | Left,
        ForwardRight = Forward | Right,
        BackwardLeft = Backward | Left,
        BackwardRight = Backward | Right
    }
}
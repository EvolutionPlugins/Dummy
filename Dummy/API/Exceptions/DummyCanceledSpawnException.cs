using OpenMod.API.Commands;

namespace Dummy.API.Exceptions
{
    public class DummyCanceledSpawnException : UserFriendlyException
    {
        public DummyCanceledSpawnException(string message) : base(message)
        {
        }
    }
}

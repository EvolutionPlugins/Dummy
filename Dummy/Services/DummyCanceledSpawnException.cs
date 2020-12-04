using OpenMod.API.Commands;

namespace Dummy.Services
{
    public class DummyCanceledSpawnException : UserFriendlyException
    {
        public DummyCanceledSpawnException(string message) : base(message)
        {
        }
    }
}

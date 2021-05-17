using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;

namespace Dummy.Actions.Movement.Actions
{
    public class RotateAction : IAction
    {
        public RotateAction(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public float Yaw { get; }
        public float Pitch { get; }

        public Task Do(DummyUser dummy)
        {
            // FIXME
            return Task.CompletedTask;
        }
    }
}
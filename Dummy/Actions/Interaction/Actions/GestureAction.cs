using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    public class GestureAction : IAction
    {
        public GestureAction(EPlayerGesture ePlayerGesture)
        {
            Gesture = ePlayerGesture;
        }

        public EPlayerGesture Gesture { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask SetGesture()
            {
                await UniTask.SwitchToMainThread();
                dummy.Player.Player.animator.sendGesture(Gesture, false);
            }

            return SetGesture().AsTask();
        }
    }
}
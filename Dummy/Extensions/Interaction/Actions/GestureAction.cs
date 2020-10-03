using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions
{
    public class GestureAction : IInteractionAction
    {
        public GestureAction(EPlayerGesture ePlayerGesture)
        {
            Gesture = ePlayerGesture;
        }

        public EPlayerGesture Gesture { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Player.Player.animator.sendGesture(Gesture, false);
        }
    }
}

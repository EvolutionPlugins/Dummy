using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
{
    public class GestureAction : IInteractionAction
    {
        public GestureAction(EPlayerGesture ePlayerGesture)
        {
            Gesture = ePlayerGesture;
        }

        public EPlayerGesture Gesture { get; }

        public async Task Do(PlayerDummy dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Player.animator.sendGesture(Gesture, false);
        }
    }
}

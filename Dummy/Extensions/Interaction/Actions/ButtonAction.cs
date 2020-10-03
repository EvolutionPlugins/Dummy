using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions
{
    public class ButtonAction : IInteractionAction
    {
        public ButtonAction(string buttonName)
        {
            ButtonName = buttonName;
        }

        public string ButtonName { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask EffectButtonClicked()
            {
                await UniTask.SwitchToMainThread();
                EffectManager.onEffectButtonClicked(dummy.Player.Player, ButtonName);
            }
            return EffectButtonClicked().AsTask();
        }
    }
}

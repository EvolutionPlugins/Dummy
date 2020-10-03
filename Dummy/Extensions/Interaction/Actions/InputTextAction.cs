using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions
{
    public class InputTextAction : IInteractionAction
    {
        public InputTextAction(string inputFieldName, string inputtedText)
        {
            InputtedText = inputtedText;
            InputFieldName = inputFieldName;
        }

        public string InputtedText { get; }
        public string InputFieldName { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask EffectTextCommitted()
            {
                await UniTask.SwitchToMainThread();
                EffectManager.onEffectTextCommitted(dummy.Player.Player, InputFieldName, InputtedText);
            }
            return EffectTextCommitted().AsTask();
        }
    }
}

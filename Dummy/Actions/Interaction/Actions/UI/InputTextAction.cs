using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions.UI
{
    public class InputTextAction : IAction
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

using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using Dummy.Extensions;

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
                var context = dummy.GetContext();
                
                EffectManager.ReceiveEffectTextCommitted(in context, InputFieldName, InputtedText);
            }
            return EffectTextCommitted().AsTask();
        }
    }
}

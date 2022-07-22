using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using Dummy.Extensions;

namespace Dummy.Actions.Interaction.Actions.UI
{
    public class ClickButtonAction : IAction
    {
        public ClickButtonAction(string buttonName)
        {
            ButtonName = buttonName;
        }

        public string ButtonName { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask EffectButtonClicked()
            {
                await UniTask.SwitchToMainThread();
                var context = dummy.GetContext();
                
                EffectManager.ReceiveEffectClicked(in context, ButtonName);
            }
            return EffectButtonClicked().AsTask();
        }
    }
}

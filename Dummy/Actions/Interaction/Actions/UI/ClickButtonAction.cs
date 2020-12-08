using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

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
                EffectManager.onEffectButtonClicked(dummy.Player.Player, ButtonName);
            }
            return EffectButtonClicked().AsTask();
        }
    }
}

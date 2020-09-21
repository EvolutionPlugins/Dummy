using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
{
    public class ButtonAction : IInteractionAction
    {
        public ButtonAction(string buttonName)
        {
            ButtonName = buttonName;
        }

        public string ButtonName { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            EffectManager.onEffectButtonClicked(dummy.Player.Player, ButtonName);
        }
    }
}

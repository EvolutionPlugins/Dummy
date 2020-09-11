using Cysharp.Threading.Tasks;
using SDG.Unturned;
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

        public async Task Do(PlayerDummy dummy)
        {
            await UniTask.SwitchToMainThread();
            EffectManager.instance.tellEffectClicked(dummy.SteamID, ButtonName);
        }
    }
}

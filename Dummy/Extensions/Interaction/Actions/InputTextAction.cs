using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
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

        public async Task Do(PlayerDummy dummy)
        {
            await UniTask.SwitchToMainThread();
            EffectManager.instance.tellEffectTextCommitted(dummy.SteamID, InputFieldName, InputtedText);
        }
    }
}

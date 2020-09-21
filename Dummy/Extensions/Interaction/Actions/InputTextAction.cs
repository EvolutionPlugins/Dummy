using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
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

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            EffectManager.onEffectTextCommitted(dummy.Player.Player, InputFieldName, InputtedText);
        }
    }
}

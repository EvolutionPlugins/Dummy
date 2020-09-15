using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using OpenMod.Core.Commands;
using System;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("button")]
    [CommandSyntax("<id> <buttonName>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyButton : CommandDummyAction
    {
        public CommandDummyButton(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider, dummyProvider)
        {
        }

        protected override UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
        {
            playerDummy.Actions.Actions.Enqueue(new ButtonAction(Context.Parameters.GetArgumentLine(1)));
            return UniTask.CompletedTask;
        }
    }
}

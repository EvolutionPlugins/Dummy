using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using EvolutionPlugins.Dummy.Models;
using OpenMod.Core.Commands;
using System;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("inputfield")]
    [CommandAlias("if")]
    [CommandSyntax("<id> <inputFieldName> <text>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyInputText : CommandDummyAction
    {
        public CommandDummyInputText(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider, dummyProvider)
        {
        }

        protected override UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
        {
            playerDummy.Actions.Actions.Enqueue(new InputTextAction(Context.Parameters[1], Context.Parameters[2]));
            return UniTask.CompletedTask;
        }
    }
}

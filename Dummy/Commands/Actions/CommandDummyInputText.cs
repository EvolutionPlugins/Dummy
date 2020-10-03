using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;

namespace Dummy.Commands.Actions
{
    [Command("inputfield")]
    [CommandAlias("if")]
    [CommandSyntax("<id> <inputFieldName> <text>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyInputText : CommandDummyAction
    {
        public CommandDummyInputText(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            playerDummy.Actions.Actions.Enqueue(new InputTextAction(Context.Parameters[1], Context.Parameters.GetArgumentLine(2)));
            return UniTask.CompletedTask;
        }
    }
}

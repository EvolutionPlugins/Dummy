using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;

namespace Dummy.Commands.Actions
{
    [Command("button")]
    [CommandSyntax("<id> <buttonName>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyButton : CommandDummyAction
    {
        public CommandDummyButton(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            playerDummy.Actions.Actions.Enqueue(new ButtonAction(Context.Parameters.GetArgumentLine(1)));
            return UniTask.CompletedTask;
        }
    }
}

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
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyButton(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var buttonName = Context.Parameters[1];
            playerDummy.Actions.Actions.Enqueue(new ButtonAction(buttonName));
            return PrintAsync(m_StringLocalizer["commands:actions:button:success", new { ButtonName = buttonName }]).AsUniTask();
        }
    }
}
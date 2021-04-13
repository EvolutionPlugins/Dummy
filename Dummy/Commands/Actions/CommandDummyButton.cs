extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions.UI;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;

namespace Dummy.Commands.Actions
{
    [Command("button")]
    [CommandSyntax("<id> <buttonName>")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
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
            if (Context.Parameters.Count != 2)
            {
                throw new CommandWrongUsageException(Context);
            }
            
            var buttonName = Context.Parameters[1];
            playerDummy.Actions.Actions.Enqueue(new ClickButtonAction(buttonName));
            return PrintAsync(m_StringLocalizer["commands:actions:button:success", new { ButtonName = buttonName }])
                .AsUniTask();
        }
    }
}
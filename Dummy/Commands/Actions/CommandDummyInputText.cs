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
    [Command("inputfield")]
    [CommandAlias("if")]
    [CommandSyntax("<id> <inputFieldName> <text>")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyInputText : CommandDummyAction
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyInputText(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (Context.Parameters.Count is < 2)
            {
                throw new CommandWrongUsageException(Context);
            }
            
            var inputFieldName = Context.Parameters[1];
            var text = Context.Parameters.GetArgumentLine(2);
            playerDummy.Actions.Actions.Enqueue(new InputTextAction(inputFieldName, text));
            return PrintAsync(m_StringLocalizer["commands:actions:inputfield:success",
                new { playerDummy.Id, Text = text, InputFieldName = inputFieldName }]).AsUniTask();
        }
    }
}
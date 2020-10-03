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
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyInputText(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var text = Context.Parameters.GetArgumentLine(2);
            var inputFieldName = Context.Parameters[1];
            playerDummy.Actions.Actions.Enqueue(new InputTextAction(inputFieldName, text));
            return PrintAsync(m_StringLocalizer["commands:actions:inputfield:success",
                new { playerDummy.Id, Text = text, InputFieldName = inputFieldName }]).AsUniTask();
        }
    }
}
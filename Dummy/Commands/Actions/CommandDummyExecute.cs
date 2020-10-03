using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Linq;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands.Actions
{
    [Command("execute")]
    [CommandDescription("Execute a command by Dummy")]
    [CommandSyntax("<id> <command>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyExecute : CommandDummyAction
    {
        private readonly ICommandExecutor m_CommandExecutor;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_CommandExecutor = commandExecutor;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var wait = false;
            playerDummy.Actions.Actions.Enqueue(new ExecuteAction(m_CommandExecutor, Context.Parameters.Skip(1).ToArray(), e => wait = true));
            await UniTask.WaitUntil(() => wait);
        }
    }
}

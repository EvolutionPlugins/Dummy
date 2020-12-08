using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
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
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_CommandExecutor = commandExecutor;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var wait = false;
            Exception exception = null;
            var command = Context.Parameters.Skip(1);
            playerDummy.Actions.Actions.Enqueue(new ExecuteCommandAction(m_CommandExecutor, command.ToArray(), e =>
            {
                exception = e;
                wait = true;
            }));
            await UniTask.WaitUntil(() => wait);
            if (exception != null)
            {
                await PrintAsync(m_StringLocalizer["commands:actions:execute:fail", new { playerDummy.Id, Command = string.Join(" ", command) }]);
            }
            else
            {
                await PrintAsync(m_StringLocalizer["commands:actions:execute:success", new { playerDummy.Id, Command = string.Join(" ", command) }]);
            }
        }
    }
}
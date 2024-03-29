﻿extern alias JetBrainsAnnotations;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Eventing;
using OpenMod.Core.Commands;

namespace Dummy.Commands.Actions
{
    [Command("execute")]
    [CommandDescription("Execute a command by Dummy")]
    [CommandSyntax("<id> <command>")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyExecute : CommandDummyAction
    {
        private readonly ICommandExecutor m_CommandExecutor;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IEventBus m_EventBus;
        private readonly Dummy m_Plugin;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor, IStringLocalizer stringLocalizer, IEventBus eventBus, Dummy plugin) :
            base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_CommandExecutor = commandExecutor;
            m_StringLocalizer = stringLocalizer;
            m_EventBus = eventBus;
            m_Plugin = plugin;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (Context.Parameters.Count < 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var wait = false;
            Exception? exception = null;
            var command = Context.Parameters.Skip(1).ToArray();

            playerDummy.Actions.Actions.Enqueue(new ExecuteCommandAction(m_CommandExecutor, command, m_Plugin,
                m_EventBus, context =>
                {
                    wait = true;
                    exception = context.Exception;
                }));
            await UniTask.WaitUntil(() => wait);
            
            if (exception != null)
            {
                await PrintAsync(m_StringLocalizer["commands:actions:execute:fail",
                    new { playerDummy.Id, Command = string.Join(" ", command) }]);
            }
            else
            {
                await PrintAsync(m_StringLocalizer["commands:actions:execute:success",
                    new { playerDummy.Id, Command = string.Join(" ", command) }]);
            }
        }
    }
}
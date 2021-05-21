extern alias JetBrainsAnnotations;
using Dummy.API;
using Dummy.Users;
using OpenMod.API.Commands;
using System;
using System.Threading.Tasks;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Commands.Events;
using OpenMod.Core.Eventing;
using OpenMod.Core.Helpers;

namespace Dummy.Actions.Interaction.Actions
{
    public class ExecuteCommandAction : IAction
    {
        private readonly ICommandExecutor m_CommandExecutor;
        private readonly IOpenModComponent? m_Component;
        private readonly IEventBus? m_EventBus;
        private readonly Action<ICommandContext>? m_CommandExecutedHandler;

        private DummyUser? m_User;
        private bool m_ExceptionHandled;

        public string[] Arguments { get; }

        public ExecuteCommandAction(ICommandExecutor commandExecutor, string[] args, IOpenModComponent? component,
            IEventBus? eventBus, Action<ICommandContext>? commandExecutedHandler)
        {
            Arguments = args;
            m_CommandExecutor = commandExecutor;
            m_Component = component;
            m_EventBus = eventBus;
            m_CommandExecutedHandler = commandExecutedHandler;
        }

        public async Task Do(DummyUser dummy)
        {
            m_User = dummy;
            IDisposable iEvent = NullDisposable.Instance;
            if (m_EventBus != null && m_Component != null)
            {
                iEvent = m_EventBus.Subscribe<CommandExecutedEvent>(m_Component, OnCommandExecutedEvent);
            }

            var commandContext = await m_CommandExecutor.ExecuteAsync(dummy, Arguments, string.Empty);
            commandContext.Exception =
                commandContext.Exception != null && m_ExceptionHandled ? null : commandContext.Exception;
            iEvent.Dispose();

            m_CommandExecutedHandler?.Invoke(commandContext);
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        private Task OnCommandExecutedEvent(IServiceProvider serviceprovider, object? sender,
            CommandExecutedEvent @event)
        {
            if (Equals(@event.Actor, m_User))
            {
                m_ExceptionHandled = @event.ExceptionHandled;
            }

            return Task.CompletedTask;
        }
    }
}
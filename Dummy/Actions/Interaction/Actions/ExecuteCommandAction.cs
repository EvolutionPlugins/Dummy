extern alias JetBrainsAnnotations;
using Dummy.API;
using Dummy.Users;
using OpenMod.API.Commands;
using System;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    public class ExecuteCommandAction : IAction
    {
        private readonly ICommandExecutor m_CommandExecutor;

        public Action<Exception?>? ExceptionHandler { get; }
        public string[] Arguments { get; }

        public ExecuteCommandAction(ICommandExecutor commandExecutor, string[] args, Action<Exception?>? exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            Arguments = args;
            m_CommandExecutor = commandExecutor;
        }

        public async Task Do(DummyUser dummy)
        {
            var commandContext = await m_CommandExecutor.ExecuteAsync(dummy, Arguments, string.Empty);
            ExceptionHandler?.Invoke(commandContext.Exception);
        }
    }
}
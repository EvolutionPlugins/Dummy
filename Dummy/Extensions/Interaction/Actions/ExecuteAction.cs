using Dummy.Users;
using OpenMod.API.Commands;
using System;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions
{
    public class ExecuteAction : IInteractionAction
    {
        private readonly ICommandExecutor m_CommandExecutor;
        public Action<Exception> ExceptionHandler { get; }
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Arguments { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        public ExecuteAction(ICommandExecutor commandExecutor, string[] args, Action<Exception> exceptionHandler)
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

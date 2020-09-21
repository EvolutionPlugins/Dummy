using EvolutionPlugins.Dummy.Models.Users;
using OpenMod.API.Commands;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
{
    public class ExecuteAction : IInteractionAction
    {
        private readonly ICommandExecutor _CommandExecutor;
        public Action<Exception> ExceptionHandler { get; }
        public string[] Arguments { get; }

        public ExecuteAction(ICommandExecutor commandExecutor, string[] args, Action<Exception> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            Arguments = args;
            _CommandExecutor = commandExecutor;
        }

        public async Task Do(DummyUser dummy)
        {
            var commandContext = await _CommandExecutor.ExecuteAsync(dummy, Arguments, string.Empty);
            ExceptionHandler?.Invoke(commandContext.Exception);
        }
    }
}

using EvolutionPlugins.Dummy.Models;
using OpenMod.API.Commands;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
{
    public class ExecuteAction : IInteractionAction
    {
        public ExecuteAction(ICommandExecutor commandExecutor, string[] args, Action<Exception> exceptionHandler)
        {
            ExceptionHandler = exceptionHandler;
            Arguments = args;
            _CommandExecutor = commandExecutor;
        }
        private readonly ICommandExecutor _CommandExecutor;
        public Action<Exception> ExceptionHandler { get; }
        public string[] Arguments { get; }

        public async Task Do(PlayerDummy dummy)
        {
            var commandContext = await _CommandExecutor.ExecuteAsync(dummy.Data.UnturnedUser, Arguments, "");
            ExceptionHandler?.Invoke(commandContext.Exception);
        }
    }
}

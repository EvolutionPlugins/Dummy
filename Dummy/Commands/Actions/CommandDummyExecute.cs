using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using EvolutionPlugins.Dummy.Models;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Linq;
using Color = System.Drawing.Color;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("execute")]
    [CommandDescription("Execute a command by Dummy")]
    [CommandSyntax("<id> <command>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyExecute : CommandDummyAction
    {
        private readonly ICommandExecutor m_CommandExecutor;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor) : base(serviceProvider, dummyProvider)
        {
            m_CommandExecutor = commandExecutor;
        }

        protected override UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
        {
            playerDummy.Actions.Actions.Enqueue(new ExecuteAction(m_CommandExecutor, Context.Parameters.Skip(1).ToArray(), async e =>
            {
                await PrintAsync($"Dummy has {(e == null ? "<color=green>successfully" : "<color=red>unsuccessfully")}</color> executed command");
                if (e != null && !(e is UserFriendlyException))
                {
                    await PrintAsync(e.Message, Color.Red);
                }
            }));
            return UniTask.CompletedTask;
        }
    }
}

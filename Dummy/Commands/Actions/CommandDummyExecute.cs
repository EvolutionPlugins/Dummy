using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("execute")]
    [CommandDescription("Execute a command by Dummy")]
    [CommandSyntax("<id> <command>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyExecute : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly ICommandExecutor m_CommandExecutor;

        public CommandDummyExecute(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            ICommandExecutor commandExecutor) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_CommandExecutor = commandExecutor;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count < 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.GetPlayerDummy(id.m_SteamID);
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }
            dummy.Actions.Actions.Enqueue(new ExecuteAction(m_CommandExecutor, Context.Parameters.Skip(1).ToArray(), async e =>
            {
                await PrintAsync($"Dummy has {(e == null ? "<color=green>successfully" : "<color=red>unsuccessfully")}</color> executed command");
                if (e != null && !(e is UserFriendlyException))
                {
                    await PrintAsync(e.Message, Color.Red);
                }
            }));
        }
    }
}

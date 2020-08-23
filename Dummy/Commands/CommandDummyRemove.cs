using EvolutionPlugins.Dummy.API;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("remove")]
    [CommandAlias("kick")]
    [CommandAlias("delete")]
    [CommandDescription("Removes a dummy by id")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id>")]
    public class CommandDummyRemove : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyRemove(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }
            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            var deleted = await m_DummyProvider.RemoveDummyAsync(id);

            await PrintAsync($"Dummy {(deleted ? "is" : "not")} kicked");
        }
    }
}

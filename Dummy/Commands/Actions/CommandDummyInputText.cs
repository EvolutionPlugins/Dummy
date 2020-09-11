using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("inputfield")]
    [CommandAlias("if")]
    [CommandSyntax("<id> <inputFieldName> <text>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyInputText : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyInputText(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count < 3)
            {
                throw new CommandWrongUsageException(Context);
            }

            var id = (CSteamID)await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.GetPlayerDummy(id.m_SteamID);
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }
            dummy.Actions.Actions.Enqueue(new InputTextAction(Context.Parameters[1], Context.Parameters[2]));
        }
    }
}

using Dummy.Extensions;
using EvolutionPlugins.Dummy.API;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("tphere")]
    [CommandDescription("Teleport to you a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyTphere : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        public CommandDummyTphere(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Count == 0)
            {
                throw new CommandWrongUsageException(Context);
            }
            var id = await Context.Parameters.GetAsync<ulong>(0);

            var dummy = await m_DummyProvider.GetPlayerDummy(id);
            if (dummy == null)
            {
                throw new UserFriendlyException($"Dummy \"{id}\" has not found!");
            }
            await dummy.Data.UnturnedUser.TeleportToPlayerAsync((UnturnedUser)Context.Actor);
        }
    }
}

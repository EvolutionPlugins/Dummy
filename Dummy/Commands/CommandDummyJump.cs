using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Commands;
using EvolutionPlugins.Dummy.Extensions.Movement;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy.Commands
{
    [Command("jump")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyJump : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyJump(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
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
            dummy.Jump();
        }
    }
}

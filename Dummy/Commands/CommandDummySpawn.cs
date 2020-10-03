using Dummy.API;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dummy.Commands
{
    [Command("spawn")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummySpawn : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummySpawn(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            var id = await m_DummyProvider.GetAvailableIdAsync();
            await m_DummyProvider.AddDummyAsync(id, new HashSet<CSteamID>());
        }
    }
}

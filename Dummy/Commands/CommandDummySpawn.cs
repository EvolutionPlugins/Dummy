extern alias JetBrainsAnnotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dummy.API;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.Core.Commands;
using Steamworks;

namespace Dummy.Commands
{
    [Command("spawn")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummySpawn : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummySpawn(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            await m_DummyProvider.AddDummyAsync(null, new HashSet<CSteamID>());
        }
    }
}
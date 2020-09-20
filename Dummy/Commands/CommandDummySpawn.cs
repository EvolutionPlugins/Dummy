using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models;
using OpenMod.Core.Commands;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("spawn")]
    public class CommandDummySpawn : Command
    {
        private readonly API.IDummyProvider dummyProvider;

        public CommandDummySpawn(IServiceProvider serviceProvider, API.IDummyProvider dummyProvider) : base(serviceProvider)
        {
            this.dummyProvider = dummyProvider;
        }

        protected override Task OnExecuteAsync()
        {
            return dummyProvider.AddDummyAsync((CSteamID)1, new HashSet<CSteamID>());
        }
    }
}

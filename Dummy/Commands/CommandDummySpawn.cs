extern alias JetBrainsAnnotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Models;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration m_Configuration;

        public CommandDummySpawn(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IConfiguration configuration) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_Configuration = configuration;
        }

        protected override async Task OnExecuteAsync()
        {
            var settings = m_Configuration.Get<Configuration>().Default;
            var name = CommandDummy.s_NameArgument.GetArgument(Context.Parameters);
            if (!string.IsNullOrEmpty(name))
            {
                settings.CharacterName = name!;
                settings.NickName = name!;
                settings.PlayerName = name!;
            }

            await m_DummyProvider.AddDummyByParameters(null, new HashSet<CSteamID>(), settings);
        }
    }
}
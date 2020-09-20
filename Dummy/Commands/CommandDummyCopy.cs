using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("copy")]
    [CommandDescription("Creates a dummy and copy your skin, hait, beard, etc...")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyCopy : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyCopy(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            var id = await m_DummyProvider.GetAvailableIdAsync();
            var playerDummy = await m_DummyProvider.AddCopiedDummyAsync(id, new HashSet<CSteamID> { user.SteamId }, user);
            await playerDummy.Data.UnturnedUser.TeleportToPlayerAsync(user);

            await user.PrintMessageAsync($"Dummy ({id.m_SteamID}) has created");
        }
    }
}

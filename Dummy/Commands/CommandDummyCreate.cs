using Dummy.API;
using Dummy.Extensions;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands
{
    [Command("create")]
    [CommandDescription("Creates a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyCreate : Command
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyCreate(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            var id = await m_DummyProvider.GetAvailableIdAsync();
            var playerDummy = await m_DummyProvider.AddDummyAsync(id, new HashSet<CSteamID> { user.SteamId });
            await playerDummy.TeleportToPlayerAsync(user);

            await PrintAsync($"Dummy {playerDummy.Id} has created");
        }
    }
}

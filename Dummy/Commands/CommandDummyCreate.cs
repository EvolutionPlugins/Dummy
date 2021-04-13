extern alias JetBrainsAnnotations;
using System;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;

namespace Dummy.Commands
{
    [Command("create")]
    [CommandDescription("Creates a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyCreate : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyCreate(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            var id = await m_DummyProvider.GetAvailableIdAsync();
            var playerDummy = await m_DummyProvider.AddDummyAsync(id, new() { user.SteamId });
            if (playerDummy == null)
            {
                throw new NotImplementedException();
            }
            await playerDummy.TeleportToPlayerAsync(user);

            await PrintAsync(m_StringLocalizer["commands:general:create", new { Id = id }]);
        }
    }
}
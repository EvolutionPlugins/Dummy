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
    [Command("copy")]
    [CommandDescription("Creates a dummy and copy your skin, hair, beard, etc...")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyCopy : Command
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyCopy(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            var playerDummy =
                await m_DummyProvider.AddCopiedDummyAsync(null, new() { user.SteamId }, user);

            await playerDummy.TeleportToPlayerAsync(user);

            await PrintAsync(m_StringLocalizer["commands:general:copy", new { Id = playerDummy.SteamId }]);
        }
    }
}
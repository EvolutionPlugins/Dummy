extern alias JetBrainsAnnotations;
using System;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Models;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration m_Configuration;

        public CommandDummyCopy(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer, IConfiguration configuration) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        protected override async Task OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;

            ConfigurationSettings? settings = null;
            var name = CommandDummy.s_NameArgument.GetArgument(Context.Parameters);
            if (!string.IsNullOrEmpty(name))
            {
                settings = m_Configuration.GetSection("default").Get<ConfigurationSettings>();

                settings.CharacterName = name!;
                settings.NickName = name!;
                settings.PlayerName = name!;
            }

            var playerDummy =
                await m_DummyProvider.AddDummyAsync(null, new() { user.SteamId }, user, settings);

            await playerDummy.TeleportToPlayerAsync(user);

            await PrintAsync(m_StringLocalizer["commands:general:copy", new { Id = playerDummy.SteamId }]);
        }
    }
}
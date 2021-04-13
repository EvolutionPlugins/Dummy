extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;

namespace Dummy.Commands
{
    [Command("tphere")]
    [CommandDescription("Teleport to you a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id>")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyTphere : CommandDummyAction
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyTphere(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (await playerDummy.TeleportToPlayerAsync((UnturnedUser)Context.Actor))
            {
                await PrintAsync(m_StringLocalizer["commands:general:tphere:success", new { playerDummy.Id }]);
            }
            else
            {
                await PrintAsync(m_StringLocalizer["commands:general:tphere:fail", new { playerDummy.Id }]);
            }
        }
    }
}
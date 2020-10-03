using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Dummy.Commands
{
    [Command("tphere")]
    [CommandDescription("Teleport to you a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyTphere : CommandDummyAction
    {
        public CommandDummyTphere(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            return playerDummy.TeleportToPlayerAsync((UnturnedUser)Context.Actor);
        }
    }
}

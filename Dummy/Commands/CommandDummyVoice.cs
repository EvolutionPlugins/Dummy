extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;

namespace Dummy.Commands
{
    [Command("voice")]
    [CommandParent(typeof(CommandDummy))]
    [CommandActor(typeof(UnturnedUser))]
    [UsedImplicitly]
    public class CommandDummyVoice : CommandDummyAction
    {
        public CommandDummyVoice(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var user = (UnturnedUser)Context.Actor;

            playerDummy.CopyUserVoice = user.Player.Player;
            return UniTask.CompletedTask;
        }
    }
}

using Cysharp.Threading.Tasks;
using Dummy.Extensions;
using EvolutionPlugins.Dummy.API;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;

namespace EvolutionPlugins.Dummy.Commands
{
    [Command("tphere")]
    [CommandDescription("Teleport to you a dummy")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyTphere : CommandDummyAction
    {
        public CommandDummyTphere(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider, dummyProvider)
        {
        }

        protected override UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
        {
            return playerDummy.Data.UnturnedUser.TeleportToPlayerAsync((UnturnedUser)Context.Actor);
        }
    }
}

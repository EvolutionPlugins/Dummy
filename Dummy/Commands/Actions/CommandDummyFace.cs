using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands.Actions
{
    [Command("face")]
    [CommandDescription("Send a face to dummy")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <face>")]
    public class CommandDummyFace : CommandDummyAction
    {
        public CommandDummyFace(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var faceId = await Context.Parameters.GetAsync<byte>(1);
            if (faceId > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new UserFriendlyException($"Can't change to {faceId} because is higher {Customization.FACES_FREE + Customization.FACES_PRO}");
            }
            playerDummy.Actions.Actions.Enqueue(new FaceAction(faceId));
        }
    }
}

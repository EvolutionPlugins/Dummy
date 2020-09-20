using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Interaction.Actions;
using EvolutionPlugins.Dummy.Models;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("face")]
    [CommandDescription("Send a face to dummy")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <face>")]
    public class CommandDummyFace : CommandDummyAction
    {
        public CommandDummyFace(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider, dummyProvider)
        {
        }

        protected override async UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
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

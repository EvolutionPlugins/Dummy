using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
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
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyFace(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var faceId = await Context.Parameters.GetAsync<byte>(1);
            playerDummy.Actions.Actions.Enqueue(new FaceAction(faceId));
            await PrintAsync(m_StringLocalizer["commands:actions:face:success", new { playerDummy.Id, FaceIndex = faceId }]);
        }
    }
}
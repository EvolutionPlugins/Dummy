extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;

namespace Dummy.Commands.Actions
{
    [Command("face")]
    [CommandDescription("Send a face to dummy")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <face>")]
    [UsedImplicitly]
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
            if (Context.Parameters.Count != 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var faceId = await Context.Parameters.GetAsync<byte>(1);
            playerDummy.Actions.Actions.Enqueue(new FaceAction(faceId));
            await PrintAsync(m_StringLocalizer["commands:actions:face:success",
                new { playerDummy.Id, FaceIndex = faceId }]);
        }
    }
}
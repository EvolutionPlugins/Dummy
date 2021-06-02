extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SDG.Unturned;

namespace Dummy.Commands.Actions
{
    [Command("gesture")]
    [CommandDescription("Make a dummy gesture")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <gesture>")]
    [UsedImplicitly]
    public class CommandDummyGesture : CommandDummyAction
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyGesture(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
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

            var gesture = Context.Parameters[1];
            if (!Enum.TryParse<EPlayerGesture>(gesture, true, out var eGesture))
            {
                await PrintAsync($"Unable find a gesture {gesture}");
                await PrintAsync($"All gestures: {string.Join(", ", Enum.GetNames(typeof(EPlayerGesture)))}");
                return;
            }

            playerDummy.Actions.Actions.Enqueue(new GestureAction(eGesture));
            await PrintAsync(m_StringLocalizer["commands:actions:gesture:success",
                new { playerDummy.Id, EGesture = eGesture }]);
        }
    }
}
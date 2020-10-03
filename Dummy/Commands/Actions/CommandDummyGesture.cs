using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands.Actions
{
    [Command("gesture")]
    [CommandDescription("Make a dummy gesture")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <gesture>")]
    public class CommandDummyGesture : CommandDummyAction
    {
        public CommandDummyGesture(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var gesture = await Context.Parameters.GetAsync<string>(1);
            if (!Enum.TryParse<EPlayerGesture>(gesture.ToUpper(), out var eGesture))
            {
                await PrintAsync($"Unable find a gesture {gesture}");
                await PrintAsync($"All gestures: {string.Join(",", Enum.GetNames(typeof(EPlayerGesture)))}");
                return;
            }
            playerDummy.Actions.Actions.Enqueue(new GestureAction(eGesture));
        }
    }
}

using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions.Movement.Actions;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using Command = OpenMod.Core.Commands.Command;

namespace Dummy.Commands.Actions
{
    [Command("stance")]
    [CommandSyntax("<id> <stance>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyStance : CommandDummyAction
    {
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDummyStance(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var stance = Context.Parameters[1];
            if (!Enum.TryParse<EPlayerStance>(stance.ToUpper(), out var eStance))
            {
                await PrintAsync($"Unable to find a stance: {stance}");
                await PrintAsync($"All stances: {string.Join(", ", Enum.GetNames(typeof(EPlayerStance)))}");
                return;
            }
            playerDummy.Actions.Actions.Enqueue(new StanceAction(eStance));
            await PrintAsync(m_StringLocalizer["commands:actions:stance:success", new { playerDummy.Id, EStance = eStance }]);
        }
    }
}
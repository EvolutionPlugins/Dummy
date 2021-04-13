extern alias JetBrainsAnnotations;
using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Movement.Actions;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SDG.Unturned;

namespace Dummy.Commands.Actions
{
    [Command("stance")]
    [CommandSyntax("<id> <stance>")]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
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
            if (Context.Parameters.Count != 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var stance = Context.Parameters[1];
            if (!Enum.TryParse<EPlayerStance>(stance, true, out var eStance))
            {
                await PrintAsync($"Unable to find a stance: {stance}");
                await PrintAsync($"All stances: {string.Join(", ", Enum.GetNames(typeof(EPlayerStance)))}");
                return;
            }

            playerDummy.Actions.Actions.Enqueue(new StanceAction(eStance));
            await PrintAsync(m_StringLocalizer["commands:actions:stance:success",
                new { playerDummy.Id, EStance = eStance }]);
        }
    }
}
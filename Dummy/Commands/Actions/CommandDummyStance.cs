using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Extensions.Movement.Actions;
using OpenMod.Core.Commands;
using SDG.Unturned;
using System;
using Command = OpenMod.Core.Commands.Command;

namespace EvolutionPlugins.Dummy.Commands.Actions
{
    [Command("stance")]
    [CommandSyntax("<id> <stance>")]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyStance : CommandDummyAction
    {
        public CommandDummyStance(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider, dummyProvider)
        {
        }

        protected override async UniTask ExecuteDummyAsync(PlayerDummy playerDummy)
        {
            var stance = Context.Parameters[1];
            if (!Enum.TryParse<EPlayerStance>(stance.ToUpper(), out var eStance))
            {
                await PrintAsync($"Unable to find a stance: {stance}");
                await PrintAsync($"All stances: {string.Join(",", Enum.GetNames(typeof(EPlayerStance)))}");
                return;
            }
            playerDummy.Actions.Actions.Enqueue(new StanceAction(eStance));
        }
    }
}

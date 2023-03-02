using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions;
using Dummy.Actions.Interaction;
using Dummy.Actions.Interaction.Actions;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SDG.Unturned;

namespace Dummy.Commands.Actions;
[Command("mouse")]
[CommandParent(typeof(CommandDummy))]
[CommandSyntax("<id> <mouse>")]
public class CommandDummyMouse : CommandDummyAction
{
    public CommandDummyMouse(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer)
        : base(serviceProvider, dummyProvider, stringLocalizer)
    {
    }

    protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
    {
        if (Context.Parameters.Count != 2)
        {
            throw new CommandWrongUsageException(Context);
        }

        if (!Enum.TryParse<MouseState>(Context.Parameters[1], true, out var state))
        {
            await PrintAsync("Unable parse a mouse state");
            await PrintAsync($"All mouse states: {string.Join(", ", Enum.GetNames(typeof(MouseState)))}");
            return;
        }

        playerDummy.Punch(state);
    }
}

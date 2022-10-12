using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions;
using Dummy.Actions.Movement.Actions;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using UnityEngine;

namespace Dummy.Commands.Actions
{
    [Command("move")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<id> <direction>")]
    public class CommandDummyMove : CommandDummyAction
    {
        public CommandDummyMove(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer)
            : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (Context.Parameters.Count <= 1)
            {
                throw new CommandWrongUsageException(Context);
            }

            StrafeAction? strafeAction;

            if (Context.Parameters.Count == 2)
            {
                var strafe = Context.Parameters[1];

                if (!Enum.TryParse<StrafeDirection>(strafe, true, out var dir))
                {
                    await PrintAsync($"Unable find a strafe direction {strafe}");
                    await PrintAsync($"All strafes: {string.Join(", ", Enum.GetNames(typeof(StrafeDirection)))}");
                    return;
                }

                strafeAction = new(dir);
            }
            else if (Context.Parameters.Count == 3)
            {
                var strafeX = await Context.Parameters.GetAsync<int>(1);
                var strafeY = await Context.Parameters.GetAsync<int>(2);

                strafeX = Mathf.Clamp(strafeX, -1, 1);
                strafeY = Mathf.Clamp(strafeY, -1, 1);

                strafeAction = new(strafeX, strafeY);
            }
            else
            {
                throw new CommandWrongUsageException(Context);
            }

            playerDummy.Actions.Actions.Enqueue(strafeAction);
        }
    }
}

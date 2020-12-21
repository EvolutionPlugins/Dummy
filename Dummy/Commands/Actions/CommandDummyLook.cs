using Cysharp.Threading.Tasks;
using Dummy.Actions.Movement.Actions;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using System;

namespace Dummy.Commands.Actions
{
    [Command("look")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("<yaw> <pitch>")]
    public class CommandDummyLook : CommandDummyAction
    {
        public CommandDummyLook(IServiceProvider serviceProvider, IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (Context.Parameters.Count < 3)
            {
                throw new CommandWrongUsageException(Context);
            }
            var yaw = await Context.Parameters.GetAsync<float>(1);
            var pitch = await Context.Parameters.GetAsync<float>(2);

            playerDummy.Actions.Actions.Enqueue(new RotateAction(yaw, pitch));
        }
    }
}

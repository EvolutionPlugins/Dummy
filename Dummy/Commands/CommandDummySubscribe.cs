using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;

namespace Dummy.Commands
{
    [Command("subscribe")]
    [CommandParent(typeof(CommandDummy))]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id> <crushUI>")]
    [CommandDescription("Allows seeing what kind UIs a dummy sees. Also, you can interact with UI.")]
    public class CommandDummySubscribe : CommandDummyAction
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummySubscribe(IServiceProvider serviceProvider,
            IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var user = (UnturnedUser)Context.Actor;
            var crushUI = await Context.Parameters.GetAsync<bool>(1);
            if(m_DummyProvider.Dummies.Any(x => x.SubscribersUI.Contains(user.SteamId)))
            {
                throw new UserFriendlyException("You can only subscribe to 1 dummy");
            }

            playerDummy.SubscribersUI.Add(user.SteamId);

            if (crushUI)
            {
                EffectManager.instance.channel.send("askEffectClearAll", user.SteamId, ESteamPacket.UPDATE_RELIABLE_BUFFER);
            }
        }
    }
}

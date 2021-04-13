extern alias JetBrainsAnnotations;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using HarmonyLib;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.NetTransport;
using SDG.Unturned;

namespace Dummy.Commands
{
    [Command("subscribe")]
    [CommandParent(typeof(CommandDummy))]
    [CommandActor(typeof(UnturnedUser))]
    [CommandSyntax("<id> <crushUI>")]
    [CommandDescription("Allows seeing what kind UIs a dummy sees. Also, you can interact with UI.")]
    [UsedImplicitly]
    public class CommandDummySubscribe : CommandDummyAction
    {
        internal static readonly ClientStaticMethod s_SendEffectClearAllMethod =
            AccessTools.StaticFieldRefAccess<EffectManager, ClientStaticMethod>("SendEffectClearAll");

        private readonly IDummyProvider m_DummyProvider;

        public CommandDummySubscribe(IServiceProvider serviceProvider,
            IDummyProvider dummyProvider,
            IStringLocalizer stringLocalizer) : base(serviceProvider, dummyProvider, stringLocalizer)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            if (Context.Parameters.Count != 2)
            {
                throw new CommandWrongUsageException(Context);
            }

            var user = (UnturnedUser)Context.Actor;
            var crushUI = await Context.Parameters.GetAsync<bool>(1);
            if (m_DummyProvider.Dummies.Any(x => x.SubscribersUI.Contains(user.SteamId)))
            {
                throw new UserFriendlyException("You can only subscribe to 1 dummy");
            }

            playerDummy.SubscribersUI.Add(user.SteamId);

            if (crushUI)
            {
                await UniTask.SwitchToMainThread();

                s_SendEffectClearAllMethod.Invoke(ENetReliability.Reliable,
                    user.Player.Player.channel.GetOwnerTransportConnection());
            }
        }
    }
}
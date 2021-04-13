extern alias JetBrainsAnnotations;
using System;
using System.Drawing;
using System.Linq;
using Cysharp.Threading.Tasks;
using Dummy.API;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.NetTransport;

namespace Dummy.Commands
{
    [Command("unsubscribe")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    [UsedImplicitly]
    public class CommandDummyUnsubscribe : UnturnedCommand
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyUnsubscribe(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(
            serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var removed = m_DummyProvider.Dummies.Aggregate(false,
                (current, dummy) => current | dummy.SubscribersUI.Remove(user.SteamId));

            if (!removed)
            {
                throw new UserFriendlyException("You're not subscribed to any of dummies");
            }

            await PrintAsync("Successfully unsubscribe from all dummies", Color.Green);
            await UniTask.SwitchToMainThread();

            CommandDummySubscribe.s_SendEffectClearAllMethod.Invoke(ENetReliability.Reliable,
                user.Player.Player.channel.GetOwnerTransportConnection());
        }
    }
}
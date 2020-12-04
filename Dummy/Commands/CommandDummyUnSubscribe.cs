using Cysharp.Threading.Tasks;
using Dummy.API;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Dummy.Commands
{
    [Command("unsubscribe")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(CommandDummy))]
    public class CommandDummyUnsubscribe : UnturnedCommand
    {
        private readonly IDummyProvider m_DummyProvider;

        public CommandDummyUnsubscribe(IServiceProvider serviceProvider, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var user = (UnturnedUser)Context.Actor;
            var removed = false;
            foreach (var dummy in m_DummyProvider.Dummies)
            {
                removed |= dummy.SubscribersUI.Remove(user.SteamId);
            }
            if (removed)
            {
                await PrintAsync("Succefully unsubscribe from all dummies", System.Drawing.Color.Green);
                return;
            }
            throw new UserFriendlyException("You're not subscribed to any of dummies");
        }
    }
}

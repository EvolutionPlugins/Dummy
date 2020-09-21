using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Users.Events;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Events
{
    public class PlayerJoinedEvent : IEventListener<UnturnedUserConnectedEvent>
    {
        private readonly IDummyProvider m_DummyProvider;

        public PlayerJoinedEvent(IDummyProvider dummyProvider)
        {
            m_DummyProvider = dummyProvider;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object sender, UnturnedUserConnectedEvent @event)
        {
            await UniTask.SwitchToMainThread();
            foreach (var dummy in m_DummyProvider.Dummies)
            {
                var packet = Utils.buildConnectionPacket(dummy.SteamPlayer, null, out var size);
                Provider.sendToClient(@event.User.Player.SteamPlayer.transportConnection, ESteamPacket.CONNECTED, packet, size);
            }
        }
    }
}

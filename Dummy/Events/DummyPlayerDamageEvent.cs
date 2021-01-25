using Dummy.API;
using Dummy.Models;
using Dummy.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Users;
using System.Threading.Tasks;

namespace Dummy.Events
{
    public class DummyPlayerDamageEvent : IEventListener<UnturnedPlayerDamagedEvent>, IEventListener<UnturnedPlayerDamagingEvent>
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;

        public DummyPlayerDamageEvent(IDummyProvider dummyProvider, IConfiguration configuration,
            IStringLocalizer stringLocalizer, IUnturnedUserDirectory unturnedUserDirectory)
        {
            m_DummyProvider = dummyProvider;
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_UnturnedUserDirectory = unturnedUserDirectory;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object sender, UnturnedPlayerDamagedEvent @event)
        {
            var dummy = await GetDummyUser(@event.Player.SteamId.m_SteamID);
            if (dummy == null)
            {
                return;
            }

            var player = m_UnturnedUserDirectory.FindUser(@event.Killer);
            if (player == null)
            {
                return;
            }

            await player.PrintMessageAsync(m_StringLocalizer["events:damaged", new { DamageAmount = @event.DamageAmount, Id = dummy.Id }]);
        }

        [EventListener(Priority = EventListenerPriority.Normal)]
        public async Task HandleEventAsync(object sender, UnturnedPlayerDamagingEvent @event)
        {
            var dummy = await GetDummyUser(@event.Player.SteamId.m_SteamID);
            if (dummy == null)
            {
                return;
            }

            @event.IsCancelled = !m_Configuration.Get<Configuration>().Events.AllowDamage;
        }

        private Task<DummyUser> GetDummyUser(ulong id)
        {
            return m_DummyProvider.GetPlayerDummyAsync(id);
        }
    }
}

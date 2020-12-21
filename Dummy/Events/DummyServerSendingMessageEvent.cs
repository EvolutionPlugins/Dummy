using Dummy.API;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Chat.Events;
using OpenMod.Unturned.Users;
using System.Threading.Tasks;

namespace Dummy.Events
{
    public class DummyServerSendingMessageEvent : IEventListener<UnturnedServerSendingMessageEvent>
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IStringLocalizer m_StringLocalizer;

        public DummyServerSendingMessageEvent(IDummyProvider dummyProvider, IUnturnedUserDirectory unturnedUserDirectory,
            IStringLocalizer stringLocalizer)
        {
            m_DummyProvider = dummyProvider;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_StringLocalizer = stringLocalizer;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object sender, UnturnedServerSendingMessageEvent @event)
        {
            var dummy = await m_DummyProvider.GetPlayerDummyAsync(@event.ToPlayer.SteamId.m_SteamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var playerOwner = m_UnturnedUserDirectory.FindUser(owner);
                if (playerOwner == null)
                {
                    continue;
                }

                await playerOwner.PrintMessageAsync(m_StringLocalizer["events:chatted", new { Text = @event.Text, dummy.Id }],
                    System.Drawing.Color.Green, @event.IsRich, @event.IconUrl);
            }
        }
    }
}

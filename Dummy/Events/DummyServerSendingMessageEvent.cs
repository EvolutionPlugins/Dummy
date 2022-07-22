extern alias JetBrainsAnnotations;
using System.Drawing;
using System.Threading.Tasks;
using Dummy.API;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Chat.Events;
using OpenMod.Unturned.Users;

namespace Dummy.Events
{
    [UsedImplicitly]
    public class DummyServerSendingMessageEvent : IEventListener<UnturnedServerSendingMessageEvent>
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfiguration m_Configuration;

        public DummyServerSendingMessageEvent(IDummyProvider dummyProvider, IUnturnedUserDirectory unturnedUserDirectory,
            IStringLocalizer stringLocalizer, IConfiguration configuration)
        {
            m_DummyProvider = dummyProvider;
            m_UnturnedUserDirectory = unturnedUserDirectory;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object? sender, UnturnedServerSendingMessageEvent @event)
        {
            if (@event.ToPlayer is null)
            {
                return;
            }

            if (!m_Configuration.GetValue<bool>("logs:enableChatReceiveLog"))
            {
                return;
            }

            var dummy = await m_DummyProvider.FindDummyUserAsync(@event.ToPlayer.SteamId.m_SteamID);
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

                await playerOwner.PrintMessageAsync(
                    m_StringLocalizer["events:chatted", new { @event.Text, dummy.Id }],
                    Color.Green, @event.IsRich, @event.IconUrl);
            }
        }
    }
}
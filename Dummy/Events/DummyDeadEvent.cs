using Cysharp.Threading.Tasks;
using Dummy.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Events
{
    public class DummyDeadEvent : IEventListener<UnturnedPlayerDeathEvent>
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUnturnedUserDirectory m_UnturnedUserDirectory;

        public DummyDeadEvent(IDummyProvider dummyProvider, IUnturnedUserDirectory unturnedUserDirectory)
        {
            m_DummyProvider = dummyProvider;
            m_UnturnedUserDirectory = unturnedUserDirectory;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object sender, UnturnedPlayerDeathEvent @event)
        {
            var dummy = await m_DummyProvider.GetPlayerDummyAsync(@event.Player.SteamId.m_SteamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = m_UnturnedUserDirectory.FindUser(owner);
                if (player == null)
                {
                    continue;
                }

                await player.PrintMessageAsync($"Dummy {@event.Player.SteamId} has died. Death reason: {@event.DeathCause.ToString().ToLower()}, killer = {@event.Instigator}. Respawning...");
            }

            UniTask.Run(() => Revive(dummy.Player));
        }

        private async UniTask Revive(UnturnedPlayer player)
        {
            await UniTask.Delay(1500);
            await UniTask.SwitchToMainThread();
            if (player.IsAlive)
            {
                return;
            }

            player.Player.life.sendRevive();
            player.Player.life.channel.send("tellRevive", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                player.Transform.Position.ToUnityVector(),
                MeasurementTool.angleToByte(player.Player.transform.rotation.eulerAngles.y)
            });
        }
    }
}

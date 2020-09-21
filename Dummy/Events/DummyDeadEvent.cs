using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Eventing;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Players.Life.Events;
using SDG.Unturned;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Events
{
    public class DummyDeadEvent : IEventListener<UnturnedPlayerDeadEvent>
    {
        private readonly IDummyProvider m_DummyProvider;
        private readonly IUserManager m_UserManager;

        public DummyDeadEvent(IDummyProvider dummyProvider, IUserManager userManager)
        {
            m_DummyProvider = dummyProvider;
            m_UserManager = userManager;
        }

        [EventListener(Priority = EventListenerPriority.Monitor)]
        public async Task HandleEventAsync(object sender, UnturnedPlayerDeadEvent @event)
        {
            var dummy = await m_DummyProvider.GetPlayerDummyAsync(@event.Player.SteamId.m_SteamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = await m_UserManager.FindUserAsync(KnownActorTypes.Player, owner.ToString(), UserSearchMode.FindById);
                if (player == null)
                {
                    continue;
                }
                await player.PrintMessageAsync($"Dummy {@event.Player.SteamId} has died. Death reason: {@event.DeathCause.ToString().ToLower()}, killer = {@event.Instigator}. Respawning...");
            }

            AsyncHelper.Schedule($"Revive dummy {@event.Player.SteamId}", () => Revive(@event.Player).AsTask());
        }

        private async UniTask Revive(UnturnedPlayer player)
        {
            await UniTask.Delay(1500);
            if (player.IsAlive) return; // double-check
            await UniTask.SwitchToMainThread();
            player.Player.life.sendRevive();
            player.Player.life.channel.send("tellRevive", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                player.Transform.Position.ToUnityVector(),
                MeasurementTool.angleToByte(player.Player.transform.rotation.eulerAngles.y)
            });
        }
    }
}

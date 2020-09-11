using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.API;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Eventing;
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players.Events.Life;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Events
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
            if (m_DummyProvider.Dummies.ContainsKey(@event.Player.SteamId))
            {
                foreach (var owner in m_DummyProvider.Dummies[@event.Player.SteamId].Data.Owners)
                {
                    var player = await m_UserManager.FindUserAsync(KnownActorTypes.Player, owner.ToString(), UserSearchMode.FindById);
                    if (player == null)
                    {
                        continue;
                    }
                    await player.PrintMessageAsync($"Dummy {@event.Player.SteamId} has died. Death reason: {@event.DeathCause.ToString().ToLower()}, killer = {@event.Instigator}. Respawning...");
                }

                async UniTask Revive()
                {
                    await UniTask.Delay(1500);
                    if (@event.Player.IsAlive) return; // double-check
                    await UniTask.SwitchToMainThread();
                    @event.Player.Player.life.sendRevive();
                    @event.Player.Player.life.channel.send("tellRevive", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                    {
                        @event.Player.Transform.Position.ToUnityVector(),
                        MeasurementTool.angleToByte(@event.Player.Player.transform.rotation.eulerAngles.y)
                    });
                }
                await Revive();
            }
        }
    }
}

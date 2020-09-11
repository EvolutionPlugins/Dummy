using Cysharp.Threading.Tasks;
using Dummy.Extensions;
using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace EvolutionPlugins.Dummy.Providers
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class DummyProvider : IDummyProvider, IAsyncDisposable
    {
        private bool m_IsDisposing;

        private readonly IPluginAccessor<Dummy> m_PluginAccessor;
        private readonly IUserManager m_UserManager;

        private readonly Dictionary<CSteamID, PlayerDummy> m_Dummies;

        public IReadOnlyDictionary<CSteamID, PlayerDummy> Dummies => m_Dummies;

        public DummyProvider(IPluginAccessor<Dummy> pluginAccessor, IUserManager userManager)
        {
            m_Dummies = new Dictionary<CSteamID, PlayerDummy>();
            m_PluginAccessor = pluginAccessor;
            m_UserManager = userManager;

            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;

            AsyncHelper.Schedule("Do not auto kick a dummies", DontAutoKickTask);
        }

        private async Task DontAutoKickTask()
        {
            while (!m_IsDisposing)
            {
                foreach (var dummy in Dummies)
                {
                    var client = dummy.Value.Data.UnturnedUser.Player.SteamPlayer;
                    client.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
                }
                await Task.Delay(5000);
            }
        }

        private async Task KickTimerTask(ulong id, uint timer)
        {
            if (timer == 0)
            {
                return;
            }

            await Task.Delay((int)(timer * 1000));

            var user = await GetPlayerDummy(id);
            if (user == null)
            {
                return;
            }
            await user.Session.DisconnectAsync();
        }

        #region Events
        protected virtual void OnServerSendingMessage(ref string text, ref Color color, SteamPlayer fromPlayer,
            SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting)
        {
            if (toPlayer == null)
            {
                return;
            }

            if (!Dummies.ContainsKey(toPlayer.playerID.steamID))
            {
                return;
            }

            var data = Dummies[toPlayer.playerID.steamID];

            foreach (var owner in data.Data.Owners)
            {
                var steamPlayerOwner = PlayerTool.getSteamPlayer(owner);
                if (steamPlayerOwner == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy {toPlayer.playerID.steamID} got message: {text}", color,
                    toPlayer: steamPlayerOwner, iconURL: iconURL, useRichTextFormatting: true);
            }
        }

        protected virtual void OnServerDisconnected(CSteamID steamID)
        {
            m_Dummies.Remove(steamID);
        }

        protected virtual void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
            {
                return;
            }
            shouldAllow = false;
            var totalTimes = parameters.times;

            if (parameters.respectArmor)
            {
                totalTimes *= DamageTool.getPlayerArmor(parameters.limb, parameters.player);
            }
            if (parameters.applyGlobalArmorMultiplier)
            {
                totalTimes *= Provider.modeConfigData.Players.Armor_Multiplier;
            }
            var totalDamage = (byte)Mathf.Min(255, parameters.damage * totalTimes);

            var killerId = parameters.killer;

            ChatManager.say(killerId, $"Amount damage to dummy: {totalDamage}", Color.green, true);
        }
        #endregion

        public async Task<PlayerDummy> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners)
        {
            if (m_Dummies.ContainsKey(id))
            {
                throw new DummyContainsException(id.m_SteamID);
            }

            var amountDummiesConfig = m_PluginAccessor.Instance.Configuration.GetSection("options:amountDummies").Get<byte>();
            if (amountDummiesConfig != 0 && Dummies.Count + 1 > amountDummiesConfig)
            {
                throw new DummyOverflowsException((byte)Dummies.Count, amountDummiesConfig);
            }

            await UniTask.SwitchToMainThread();

            Provider.pending.Add(new SteamPending(
                    new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil), true, 0, 0, 0,
                    Color.white, Color.white, Color.white, false, 0UL, 0UL, 0UL, 0UL,
                    0UL, 0UL, 0UL, Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

            Provider.accept(new SteamPlayerID(id, 1, "dummy", "dummy", "dummy", CSteamID.Nil), true, false, 0, 0, 0,
                Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0,
                0, Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>(), EPlayerSkillset.NONE, "english",
                CSteamID.Nil);

            await UniTask.SwitchToTaskPool();

            var user = (UnturnedUser)await m_UserManager.FindUserAsync(KnownActorTypes.Player, id.ToString(), UserSearchMode.FindById);
            var playerDummyData = new PlayerDummyData(owners, user);
            var playerDummy = new PlayerDummy(playerDummyData);
            AddDummy(playerDummy);

            return playerDummy;
        }

        public Task<PlayerDummy> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy)
        {
            // todo: implement method
            throw new NotImplementedException();
        }

        private void AddDummy(PlayerDummy playerDummy)
        {
            var kickTimer = m_PluginAccessor.Instance.Configuration.GetSection("KickDummyAfterSeconds").Get<uint>();
            if (kickTimer != 0)
            {
                AsyncHelper.Schedule("Kick a dummy timer", () => KickTimerTask(playerDummy.Data.UnturnedUser.SteamId.m_SteamID, kickTimer));
            }

            m_Dummies.Add(playerDummy.Data.UnturnedUser.SteamId, playerDummy);
        }

        public async Task<bool> RemoveDummyAsync(CSteamID id)
        {
            if (m_Dummies.ContainsKey(id))
            {
                await UniTask.SwitchToMainThread();
                Provider.kick(id, "");

                m_Dummies[id].Dispose();
                m_Dummies.Remove(id);
                return true;
            }

            return false;
        }

        public async Task ClearDummies()
        {
            foreach (var steamID in m_Dummies.Keys)
            {
                await RemoveDummyAsync(steamID);
            }
        }

        public Task<CSteamID> GetAvailableIdAsync()
        {
            var result = new CSteamID(1);

            while (Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return Task.FromResult(result);
        }

        public Task<PlayerDummy> GetPlayerDummy(ulong id)
        {
            return Task.FromResult(Dummies.Values.FirstOrDefault(p => p.Data.UnturnedUser.Id == id.ToString()));
        }

        public Task<bool> GetDummyDataAsync(ulong Id, out PlayerDummyData playerDummyData)
        {
            playerDummyData = null;
            var result = Task.FromResult(m_Dummies.TryGetValue((CSteamID)Id, out PlayerDummy dummy));
            if (dummy != null)
            {
                playerDummyData = dummy.Data;
            }

            return result;
        }

        public async Task MoveDummy(ulong id, Vector3 position, float rotation)
        {
            var dummy = await GetPlayerDummy(id);
            dummy?.Data.UnturnedUser.TeleportToLocationAsync(position, rotation);
        }

        public ValueTask DisposeAsync()
        {
            if (m_IsDisposing)
            {
                return new ValueTask(Task.CompletedTask);
            }
            m_IsDisposing = true;

            Provider.onServerDisconnected -= OnServerDisconnected;
            ChatManager.onServerSendingMessage -= OnServerSendingMessage;
            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;

            return new ValueTask(ClearDummies());
        }
    }
}

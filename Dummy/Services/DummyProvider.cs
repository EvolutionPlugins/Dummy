using Autofac;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.NetTransports;
using Dummy.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Dummy.Providers
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class DummyProvider : IDummyProvider, IAsyncDisposable
    {
        private readonly HashSet<DummyUser> m_Dummies;
        private readonly IPluginAccessor<Dummy> m_PluginAccessor;
        private readonly IUserManager m_UserManager;
        private readonly IUserDataStore m_UserDataStore;
        private readonly ILogger<DummyProvider> m_Logger;
        private readonly ILoggerFactory m_LoggerFactory;

        private IStringLocalizer m_StringLocalizer;
        private bool m_IsDisposing;

        public IReadOnlyCollection<DummyUser> Dummies => m_Dummies;

        public DummyProvider(IPluginAccessor<Dummy> pluginAccessor, IUserManager userManager,
            IUserDataStore userDataStore, ILogger<DummyProvider> logger, ILoggerFactory loggerFactory)
        {
            m_Dummies = new HashSet<DummyUser>();
            m_PluginAccessor = pluginAccessor;
            m_UserManager = userManager;
            m_UserDataStore = userDataStore;
            m_Logger = logger;
            m_LoggerFactory = loggerFactory;

            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;

            AsyncHelper.Schedule("Do not auto kick a dummies", DontAutoKickTask);
        }

        private async Task DontAutoKickTask()
        {
            while (!m_IsDisposing)
            {
                //m_Logger.LogTrace("Heartbeat dummies");
                foreach (var dummy in Dummies)
                {
                    var client = dummy.SteamPlayer;
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
            m_Logger.LogDebug($"Start kick timer, will kicked after {timer} sec");
            await Task.Delay(TimeSpan.FromSeconds(timer));

            var user = await GetPlayerDummyAsync(id);
            if (user == null)
            {
                return;
            }
            m_Logger.LogDebug($"[Kick timer] => Kick dummy {id}");
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

            var dummy = Dummies.FirstOrDefault(d => d.SteamID == toPlayer.playerID.steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var steamPlayerOwner = PlayerTool.getSteamPlayer(owner);
                if (steamPlayerOwner == null)
                {
                    continue;
                }
                m_StringLocalizer ??= m_PluginAccessor.Instance.LifetimeScope.Resolve<IStringLocalizer>();
                ChatManager.serverSendMessage(m_StringLocalizer["events:chatted", new { Text = text, dummy.Id }], color,
                    toPlayer: steamPlayerOwner, iconURL: iconURL, useRichTextFormatting: true);
            }
        }

        protected virtual void OnServerDisconnected(CSteamID steamID)
        {
            AsyncHelper.RunSync(() => RemoveDummyAsync(steamID));
        }

        // todo: rewrite to use openmod event (Monitor)
        protected virtual void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            var steamId = parameters.player.channel.owner.playerID.steamID;
            if (!Dummies.Any(x => x.SteamID == steamId))
            {
                return;
            }
            shouldAllow = m_PluginAccessor.Instance.Configuration.GetSection("events:allowDamage").Get<bool>();
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
            m_StringLocalizer ??= m_PluginAccessor.Instance.LifetimeScope.Resolve<IStringLocalizer>();
            ChatManager.say(killerId, m_StringLocalizer["events:damaged", new { DamageAmount = totalDamage, Id = steamId }], Color.green, true);
        }

        #endregion Events

        private void CheckSpawn(CSteamID id)
        {
            m_StringLocalizer ??= m_PluginAccessor.Instance.LifetimeScope.Resolve<IStringLocalizer>();
            if (m_Dummies.Any(x => x.SteamID == id))
            {
                throw new DummyContainsException(m_StringLocalizer, id.m_SteamID);
            }

            var amountDummiesConfig = m_PluginAccessor.Instance.Configuration.GetSection("options:amountDummies").Get<byte>();
            if (amountDummiesConfig != 0 && Dummies.Count + 1 > amountDummiesConfig)
            {
                throw new DummyOverflowsException(m_StringLocalizer, (byte)Dummies.Count, amountDummiesConfig);
            }
        }

        public async Task<DummyUser> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners)
        {
            CheckSpawn(id);

            await UniTask.SwitchToMainThread();

            var dummyPlayerID = new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil);

            Provider.pending.Add(new SteamPending(NullTransportConnection.Instance, dummyPlayerID,
                true, 0, 0, 0, Color.white, Color.white, Color.white, false, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL,
                Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

            Provider.accept(dummyPlayerID, true, false, 0,
                0, 0, Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, Array.Empty<int>(), Array.Empty<string>(),
                Array.Empty<string>(), EPlayerSkillset.NONE, "english", CSteamID.Nil);

            var playerDummy = new DummyUser((UnturnedUserProvider)m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, m_StringLocalizer, owners);
            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public async Task<DummyUser> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy)
        {
            CheckSpawn(id);

            await UniTask.SwitchToMainThread();

            var userSteamPlayer = userCopy.Player.SteamPlayer;
            var dummyPlayerID = new SteamPlayerID(id, userSteamPlayer.playerID.characterID, "dummy", "dummy", "dummy",
                userSteamPlayer.playerID.group);

            Provider.pending.Add(new SteamPending(NullTransportConnection.Instance, dummyPlayerID, true,
                userSteamPlayer.face, userSteamPlayer.hair, userSteamPlayer.beard, userSteamPlayer.skin,
                userSteamPlayer.color, Color.white, userSteamPlayer.hand, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL,
                Array.Empty<ulong>(), EPlayerSkillset.NONE, "english", CSteamID.Nil));

            Provider.accept(dummyPlayerID, userSteamPlayer.isPro, false, userSteamPlayer.face, userSteamPlayer.hair,
                userSteamPlayer.beard, userSteamPlayer.skin, userSteamPlayer.color, userSteamPlayer.markerColor,
                userSteamPlayer.hand, userSteamPlayer.shirtItem, userSteamPlayer.pantsItem, userSteamPlayer.hatItem,
                userSteamPlayer.backpackItem, userSteamPlayer.vestItem, userSteamPlayer.maskItem,
                userSteamPlayer.glassesItem, userSteamPlayer.skinItems, userSteamPlayer.skinTags,
                userSteamPlayer.skinDynamicProps, userSteamPlayer.skillset, userSteamPlayer.language,
                userSteamPlayer.lobbyID);

            var playerDummy = new DummyUser((UnturnedUserProvider)m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, m_StringLocalizer, owners);
            PostAddDummy(playerDummy);

            return playerDummy;
        }

        private void PostAddDummy(DummyUser playerDummy)
        {
            var kickTimer = m_PluginAccessor.Instance.Configuration.GetSection("options:kickDummyAfterSeconds").Get<uint>();
            if (kickTimer != 0)
            {
                AsyncHelper.Schedule("Kick a dummy timer", () => KickTimerTask(playerDummy.SteamID.m_SteamID, kickTimer));
            }

            m_Dummies.Add(playerDummy);
        }

        public async Task<bool> RemoveDummyAsync(CSteamID id)
        {
            var playerDummy = await GetPlayerDummyAsync(id.m_SteamID);
            if (playerDummy == null)
            {
                return false;
            }
            await UniTask.SwitchToMainThread();
            await playerDummy.DisposeAsync();
            m_Dummies.Remove(playerDummy);

            return true;
        }

        public async Task ClearDummiesAsync()
        {
            await m_Dummies.DisposeAllAsync();
            m_Dummies.Clear();
        }

        public Task<CSteamID> GetAvailableIdAsync()
        {
            var result = new CSteamID(1);

            while (Dummies.Any(x => x.SteamID == result))
            {
                result.m_SteamID++;
            }
            return Task.FromResult(result);
        }

        public Task<DummyUser> GetPlayerDummyAsync(ulong id)
        {
            return Task.FromResult(Dummies.FirstOrDefault(p => p.SteamID.m_SteamID == id));
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
            return new ValueTask(ClearDummiesAsync());
        }
    }
}
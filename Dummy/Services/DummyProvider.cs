using Autofac;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Models;
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
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Dummy.Services
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
        private readonly ITransportConnection m_TransportConnection;

        private IStringLocalizer m_StringLocalizer => m_PluginAccessor.Instance.LifetimeScope.Resolve<IStringLocalizer>();
        private IConfiguration m_Configuration => m_PluginAccessor.Instance.Configuration;
        private bool m_IsDisposing;

        public IReadOnlyCollection<DummyUser> Dummies => m_Dummies;

        public DummyProvider(IPluginAccessor<Dummy> pluginAccessor, IUserManager userManager,
            IUserDataStore userDataStore, ILogger<DummyProvider> logger, ILoggerFactory loggerFactory,
            ITransportConnection transportConnection)
        {
            m_Dummies = new HashSet<DummyUser>();
            m_PluginAccessor = pluginAccessor;
            m_UserManager = userManager;
            m_UserDataStore = userDataStore;
            m_Logger = logger;
            m_LoggerFactory = loggerFactory;
            m_TransportConnection = transportConnection;
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
            shouldAllow = m_Configuration.Get<Configuration>().Events.AllowDamage;
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
            ChatManager.say(killerId, m_StringLocalizer["events:damaged", new { DamageAmount = totalDamage, Id = steamId }], Color.green, true);
        }

        #endregion Events

        private void CheckSpawn(CSteamID id)
        {
            if (m_Dummies.Any(x => x.SteamID == id))
            {
                throw new DummyContainsException(m_StringLocalizer, id.m_SteamID);
            }

            var amountDummiesConfig = m_Configuration.Get<Configuration>().Options.AmountDummies;
            if (amountDummiesConfig != 0 && Dummies.Count + 1 > amountDummiesConfig)
            {
                throw new DummyOverflowsException(m_StringLocalizer, (byte)Dummies.Count, amountDummiesConfig);
            }
        }

        public async Task<DummyUser> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners)
        {
            await UniTask.SwitchToMainThread();

            CheckSpawn(id);

            var config = m_Configuration.Get<Configuration>();
            var @default = config.Default;
            var skins = config.Default.Skins;

            var dummyPlayerID = new SteamPlayerID(id, @default.CharacterId, @default.PlayerName,
                @default.CharacterName, @default.NickName, @default.SteamGroupId, @default.HWID.GetBytes());

            // todo: skins are VERY HARD to implement ;(
            var pending = new SteamPending(m_TransportConnection, dummyPlayerID, @default.IsPro, @default.FaceId,
                @default.HairId, @default.BeardId, @default.SkinColor.ToColor(), @default.Color.ToColor(),
                @default.MarkerColor.ToColor(), @default.IsLeftHanded, skins.Shirt, skins.Pants, skins.Hat,
                skins.Backpack, skins.Vest, skins.Mask, skins.Glasses, Array.Empty<ulong>(), @default.PlayerSkillset,
                @default.Language, @default.LobbyId)
            {
                hasAuthentication = true,
                hasGroup = true,
                hasProof = true
            };

            if (config.Events.CallOnCheckValidWithExplanation)
            {
                var isValid = true;
                var explanation = string.Empty;
                try
                {
                    Provider.onCheckValidWithExplanation(new ValidateAuthTicketResponse_t
                    {
                        m_SteamID = id,
                        m_eAuthSessionResponse = EAuthSessionResponse.k_EAuthSessionResponseOK,
                        m_OwnerSteamID = id
                    }, ref isValid, ref explanation);
                }
                catch (Exception e)
                {
                    m_Logger.LogError(e, "Plugin raised an exception from onCheckValidWithExplanation: ");
                }
                if (!isValid)
                {
                    Provider.pending.RemoveAt(Provider.pending.Count - 1);
                    throw new DummyCanceledSpawnException($"Plugin reject connection a dummy({id}). Reason: {explanation}");
                }
            }

            Provider.accept(dummyPlayerID, @default.IsPro, false, @default.FaceId, @default.HairId, @default.BeardId,
                @default.SkinColor.ToColor(), @default.Color.ToColor(), @default.MarkerColor.ToColor(),
                @default.IsLeftHanded, skins.Shirt, skins.Pants, skins.Hat, skins.Backpack, skins.Vest, skins.Mask,
                skins.Glasses, Array.Empty<int>(), Array.Empty<string>(), Array.Empty<string>(), @default.PlayerSkillset,
                @default.Language, @default.LobbyId);

            var playerDummy = new DummyUser((UnturnedUserProvider)m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, m_StringLocalizer, config.Options.DisableSimulations, owners);

            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public async Task<DummyUser> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy)
        {
            await UniTask.SwitchToMainThread();

            CheckSpawn(id);

            var config = m_Configuration.Get<Configuration>();
            var userSteamPlayer = userCopy.Player.SteamPlayer;

            // todo: maybe, also copy nickname?
            var dummyPlayerID = new SteamPlayerID(id, userSteamPlayer.playerID.characterID, "dummy", "dummy", "dummy",
                userSteamPlayer.playerID.group, userSteamPlayer.playerID.hwid);

            var pending = new SteamPending(m_TransportConnection, dummyPlayerID, userSteamPlayer.isPro,
                userSteamPlayer.face, userSteamPlayer.hair, userSteamPlayer.beard, userSteamPlayer.skin,
                userSteamPlayer.color, userSteamPlayer.markerColor, userSteamPlayer.hand,
                (ulong)userSteamPlayer.shirtItem, (ulong)userSteamPlayer.pantsItem, (ulong)userSteamPlayer.hatItem,
                (ulong)userSteamPlayer.backpackItem, (ulong)userSteamPlayer.vestItem, (ulong)userSteamPlayer.maskItem,
                (ulong)userSteamPlayer.glassesItem, Array.Empty<ulong>(), userSteamPlayer.skillset,
                userSteamPlayer.language, userSteamPlayer.lobbyID)
            {
                hasProof = true,
                hasGroup = true,
                hasAuthentication = true
            };

            Provider.pending.Add(pending);

            if (config.Events.CallOnCheckValidWithExplanation)
            {
                var isValid = true;
                var explanation = string.Empty;
                try
                {
                    Provider.onCheckValidWithExplanation(new ValidateAuthTicketResponse_t
                    {
                        m_SteamID = id,
                        m_eAuthSessionResponse = EAuthSessionResponse.k_EAuthSessionResponseOK,
                        m_OwnerSteamID = id
                    }, ref isValid, ref explanation);
                }
                catch (Exception e)
                {
                    m_Logger.LogError(e, "Plugin raised an exception from onCheckValidWithExplanation: ");
                }
                if (!isValid)
                {
                    Provider.pending.RemoveAt(Provider.pending.Count - 1);
                    throw new DummyCanceledSpawnException($"Plugin reject connection a dummy({id}). Reason: {explanation}");
                }
            }

            Provider.accept(dummyPlayerID, userSteamPlayer.isPro, false, userSteamPlayer.face, userSteamPlayer.hair,
                userSteamPlayer.beard, userSteamPlayer.skin, userSteamPlayer.color, userSteamPlayer.markerColor,
                userSteamPlayer.hand, userSteamPlayer.shirtItem, userSteamPlayer.pantsItem, userSteamPlayer.hatItem,
                userSteamPlayer.backpackItem, userSteamPlayer.vestItem, userSteamPlayer.maskItem,
                userSteamPlayer.glassesItem, userSteamPlayer.skinItems, userSteamPlayer.skinTags,
                userSteamPlayer.skinDynamicProps, userSteamPlayer.skillset, userSteamPlayer.language,
                userSteamPlayer.lobbyID);

            var playerDummy = new DummyUser((UnturnedUserProvider)m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, m_StringLocalizer, config.Options.DisableSimulations, owners);

            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public Task<DummyUser> AddDummyByParameters(CSteamID id, HashSet<CSteamID> owners, ConfigurationSettings settings)
        {
            throw new NotImplementedException();
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
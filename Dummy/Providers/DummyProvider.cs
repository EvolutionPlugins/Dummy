using Autofac;
using Cysharp.Threading.Tasks;
using Dummy.API;
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
using OpenMod.Core.Users;
using OpenMod.UnityEngine.Extensions;
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
    public class DummyProvider : IDummyProvider, IUserProvider, IAsyncDisposable
    {
        private readonly HashSet<DummyUser> m_Dummies;
        private readonly IPluginAccessor<Dummy> m_PluginAccessor;
        private readonly IUserDataStore m_UserDataStore;
        private readonly ILogger<DummyProvider> m_Logger;

        private IReadOnlyCollection<IUser> Users => m_Dummies.OfType<IUser>().ToList().AsReadOnly();
        private IStringLocalizer m_StringLocalizer;
        private bool m_IsDisposing;

        public IReadOnlyCollection<DummyUser> Dummies => m_Dummies;

        public DummyProvider(IPluginAccessor<Dummy> pluginAccessor, IUserDataStore userDataStore, ILogger<DummyProvider> logger)
        {
            m_Dummies = new HashSet<DummyUser>();
            m_PluginAccessor = pluginAccessor;
            m_UserDataStore = userDataStore;
            m_Logger = logger;

            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;

            AsyncHelper.Schedule("Do not auto kick a dummies", DontAutoKickTask);
        }

        private async Task DontAutoKickTask()
        {
            while (!m_IsDisposing)
            {
                m_Logger.LogTrace("Heartbeat dummies");
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
            m_Logger.LogTrace($"Start kick timer, will kicked after {timer * 1000} sec");
            await Task.Delay((int)(timer * 1000));

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

            var index = Provider.clients.Count;
            var dummyPlayerID = new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil);

            string characterName = dummyPlayerID.characterName;
            const uint uScore = 1;
            SteamGameServer.BUpdateUserData(id, characterName, uScore);

            Utils.loadPlayerSpawn(dummyPlayerID, out var point, out var angle, out var initialStance);
            int channel = Utils.allocPlayerChannelId();
            var shouldCallOriginalEvent = m_PluginAccessor.Instance.Configuration.GetSection("events:callOriginalConnectEvent").Get<bool>();
            var dummySteamPlayer = Utils.addPlayer(dummyPlayerID, point, angle, true, false, channel, 0, 0, 0,
                Color.white, Color.white, Color.white, false, 0, 0, 0, 0, 0, 0, 0, Array.Empty<int>(),
                Array.Empty<string>(), Array.Empty<string>(), EPlayerSkillset.NONE, "english", CSteamID.Nil, shouldCallOriginalEvent);

            PreAddDummy(index, initialStance, dummySteamPlayer);

            await UniTask.SwitchToTaskPool();

            var playerDummy = new DummyUser(this, m_UserDataStore, dummySteamPlayer, index, owners);
            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public async Task<DummyUser> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy)
        {
            CheckSpawn(id);

            await UniTask.SwitchToMainThread();

            var index = Provider.clients.Count;
            var userSteamPlayer = userCopy.Player.SteamPlayer;
            var dummyPlayerID = new SteamPlayerID(id, 0, "dummy", "dummy", "dummy", CSteamID.Nil);

            string characterName = dummyPlayerID.characterName;
            const uint uScore = 1;
            SteamGameServer.BUpdateUserData(id, characterName, uScore);

            Utils.loadPlayerSpawn(dummyPlayerID, out var point, out var angle, out var initialStance);
            int channel = Utils.allocPlayerChannelId();
            var shouldCallOriginalEvent = m_PluginAccessor.Instance.Configuration.GetSection("events:callOriginalConnectEvent").Get<bool>();
            var dummySteamPlayer = Utils.addPlayer(dummyPlayerID, point, angle, true, false, channel,
                userSteamPlayer.face, userSteamPlayer.hair, userSteamPlayer.beard, userSteamPlayer.skin, userSteamPlayer.color, Color.white,
                userSteamPlayer.hand, userSteamPlayer.shirtItem, userSteamPlayer.pantsItem, userSteamPlayer.hatItem,
                userSteamPlayer.backpackItem, userSteamPlayer.vestItem, userSteamPlayer.maskItem, userSteamPlayer.glassesItem,
                userSteamPlayer.skinItems, userSteamPlayer.skinTags, userSteamPlayer.skinDynamicProps, EPlayerSkillset.NONE,
                "english", CSteamID.Nil, shouldCallOriginalEvent);

            PreAddDummy(index, initialStance, dummySteamPlayer);

            await UniTask.SwitchToTaskPool();

            var playerDummy = new DummyUser(this, m_UserDataStore, dummySteamPlayer, index, owners);
            PostAddDummy(playerDummy);

            return playerDummy;
        }

        private void PreAddDummy(int index, EPlayerStance stance, SteamPlayer steamPlayer)
        {
            var component = steamPlayer.player.stance;
            if (component != null)
            {
                component.initialStance = stance;
            }
            else
            {
                m_Logger.LogWarning("Was unable to get PlayerStance for new connection!");
            }
            steamPlayer.isAdmin = m_PluginAccessor.Instance.Configuration.GetSection("options:isAdmin").Get<bool>();
            // sending to players a dummy connected
            var packet = Utils.buildConnectionPacket(steamPlayer, null, out var size);
            foreach (var client in Provider.clients)
            {
                Provider.sendToClient(client.transportConnection, ESteamPacket.CONNECTED, packet, size);
            }

            if (m_PluginAccessor.Instance.Configuration.GetSection("events:callOriginalConnectEvent").Get<bool>())
            {
                try
                {
                    Provider.onServerConnected?.Invoke(steamPlayer.playerID.steamID);
                }
                catch (Exception e)
                {
                    m_Logger.LogWarning("Plugin raised an exception from onServerConnected:");
                    m_Logger.LogError(e.ToString());
                }
            }

            if (CommandWindow.shouldLogJoinLeave)
            {
                CommandWindow.Log(Provider.localization.format("PlayerConnectedText", new object[]
                {
                    steamPlayer.playerID.steamID,
                    steamPlayer.playerID.playerName,
                    steamPlayer.playerID.characterName
                }));
            }
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

        public Task ClearDummiesAsync()
        {
            return m_Dummies.DisposeAllAsync();
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

        public bool SupportsUserType(string userType)
        {
            return userType.Equals(KnownActorTypes.Player, StringComparison.OrdinalIgnoreCase);
        }

        public Task<IUser> FindUserAsync(string userType, string searchString, UserSearchMode searchMode)
        {
            var confidence = 0;
            var unturnedUser = (IUser)null;

            foreach (var user in m_Dummies)
            {
                switch (searchMode)
                {
                    case UserSearchMode.FindByNameOrId:
                    case UserSearchMode.FindById:
                        if (user.Id.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                            return Task.FromResult((IUser)user);

                        if (searchMode == UserSearchMode.FindByNameOrId)
                            goto case UserSearchMode.FindByName;
                        break;

                    case UserSearchMode.FindByName:
                        var currentConfidence = NameConfidence(user.DisplayName, searchString, confidence);
                        if (currentConfidence > confidence)
                        {
                            unturnedUser = user;
                            confidence = currentConfidence;
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null);
                }
            }

            return Task.FromResult(unturnedUser);
        }

        private int NameConfidence(string userName, string searchName, int currentConfidence = -1)
        {
            switch (currentConfidence)
            {
                case 2:
                    if (userName.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                        return 3;
                    goto case 1;

                case 1:
                    if (userName.StartsWith(searchName, StringComparison.OrdinalIgnoreCase))
                        return 2;
                    goto case 0;

                case 0:
                    if (userName.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) != -1)
                        return 1;
                    break;

                default:
                    goto case 2;
            }

            return -1;
        }

        public Task<IReadOnlyCollection<IUser>> GetUsersAsync(string userType)
        {
            return Task.FromResult(Users);
        }

        public Task BroadcastAsync(string userType, string message, System.Drawing.Color? color = null)
        {
            if (!KnownActorTypes.Player.Equals(userType, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            return BroadcastAsync(message, color);
        }

        public Task BroadcastAsync(string message, System.Drawing.Color? color = null)
        {
            return BroadcastAsync(message, color, true, null);
        }

        private Task BroadcastAsync(string message, System.Drawing.Color? color, bool isRich, string iconUrl)
        {
            async UniTask BroadcastTask()
            {
                await UniTask.SwitchToMainThread();
                color ??= System.Drawing.Color.White;
                ChatManager.serverSendMessage(text: message, color: color.Value.ToUnityColor(), useRichTextFormatting: isRich, iconURL: iconUrl);
            }

            return BroadcastTask().AsTask();
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
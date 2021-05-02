extern alias JetBrainsAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Models;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
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
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dummy.Services
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    [UsedImplicitly]
    public class DummyProvider : IDummyProvider, IAsyncDisposable
    {
        private readonly HashSet<DummyUser> m_Dummies;
        private readonly IPluginAccessor<Dummy> m_PluginAccessor;
        private readonly IUserManager m_UserManager;
        private readonly IUserDataStore m_UserDataStore;
        private readonly ILogger<DummyProvider> m_Logger;
        private readonly ILoggerFactory m_LoggerFactory;
        private readonly ITransportConnection m_TransportConnection;

        private IStringLocalizer StringLocalizer =>
            m_PluginAccessor.Instance!.LifetimeScope.Resolve<IStringLocalizer>();

        private IConfiguration Configuration => m_PluginAccessor.Instance!.Configuration;
        private bool m_IsDisposing;

        public IReadOnlyCollection<DummyUser> Dummies => m_Dummies;

        public DummyProvider(IPluginAccessor<Dummy> pluginAccessor, IUserManager userManager,
            IUserDataStore userDataStore, ILogger<DummyProvider> logger, ILoggerFactory loggerFactory,
            ITransportConnection transportConnection)
        {
            m_Dummies = new();
            m_PluginAccessor = pluginAccessor;
            m_UserManager = userManager;
            m_UserDataStore = userDataStore;
            m_Logger = logger;
            m_LoggerFactory = loggerFactory;
            m_TransportConnection = transportConnection;

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

        private async Task KickTimerTask(ulong id, float timer)
        {
            if (timer == 0)
            {
                return;
            }

            m_Logger.LogDebug("Start kick timer, will kicked after {Timer} sec", timer);
            await Task.Delay(TimeSpan.FromSeconds(timer));

            var user = await GetPlayerDummyAsync(id);
            if (user?.Session == null)
            {
                return;
            }

            m_Logger.LogDebug("[Kick timer] => Kick dummy {Id}", user.Id);
            await user.Session.DisconnectAsync();
        }
        
        [AssertionMethod]
        private void ValidateSpawn(CSteamID id)
        {
            if (m_Dummies.Any(x => x.SteamID == id))
            {
                throw new DummyContainsException(StringLocalizer, id.m_SteamID);
            }

            var amountDummiesConfig = Configuration.Get<Configuration>().Options?.AmountDummies ?? 0;
            if (amountDummiesConfig != 0 && Dummies.Count + 1 > amountDummiesConfig)
            {
                throw new DummyOverflowsException(StringLocalizer, (byte)Dummies.Count, amountDummiesConfig);
            }
        }

        public async Task<DummyUser?> AddDummyAsync(CSteamID id, HashSet<CSteamID> owners)
        {
            await UniTask.SwitchToMainThread();

            ValidateSpawn(id);

            var config = Configuration.Get<Configuration>();
            var @default = config.Default ?? throw new ArgumentException(nameof(config.Default));
            var skins = config.Default?.Skins ?? throw new ArgumentException(nameof(config.Default.Skins));
            var options = config.Options ?? throw new ArgumentException(nameof(config.Options));

            var dummyPlayerID = new SteamPlayerID(id, @default.CharacterId, @default.PlayerName,
                @default.CharacterName, @default.NickName, @default.SteamGroupId, @default.Hwid.GetBytes());


            var skinColor = @default.SkinColor?.ToColor() ?? Color.white;
            var color = @default.Color?.ToColor() ?? Color.white;
            var markerColor = @default.MarkerColor?.ToColor() ?? Color.white;

            // todo: skins are VERY HARD to implement ;(
            var pending = new SteamPending(m_TransportConnection, dummyPlayerID, @default.IsPro, @default.FaceId,
                @default.HairId, @default.BeardId, skinColor, color, markerColor,
                @default.IsLeftHanded, skins.Shirt, skins.Pants, skins.Hat,
                skins.Backpack, skins.Vest, skins.Mask, skins.Glasses, Array.Empty<ulong>(),
                @default.PlayerSkillset, @default.Language, @default.LobbyId)
            {
                hasAuthentication = true,
                hasGroup = true,
                hasProof = true
            };

            Provider.pending.Add(pending);

            PreAddDummy(dummyPlayerID);

            Provider.accept(dummyPlayerID, @default.IsPro, false, @default.FaceId, @default.HairId, @default.BeardId,
                skinColor, color, markerColor, @default.IsLeftHanded, skins.Shirt, skins.Pants, skins.Hat,
                skins.Backpack,
                skins.Vest, skins.Mask, skins.Glasses, Array.Empty<int>(), Array.Empty<string>(),
                Array.Empty<string>(), @default.PlayerSkillset, @default.Language, @default.LobbyId);

            var playerDummy = new DummyUser(
                (UnturnedUserProvider)(m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider)
                                       ?? throw new InvalidOperationException()),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, StringLocalizer,
                options.DisableSimulations, owners);

            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public async Task<DummyUser?> AddCopiedDummyAsync(CSteamID id, HashSet<CSteamID> owners, UnturnedUser userCopy)
        {
            await UniTask.SwitchToMainThread();

            ValidateSpawn(id);

            var config = Configuration.Get<Configuration>();
            var options = config.Options ?? throw new ArgumentException(nameof(config.Options));
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

            PreAddDummy(dummyPlayerID);

            Provider.accept(dummyPlayerID, userSteamPlayer.isPro, false, userSteamPlayer.face, userSteamPlayer.hair,
                userSteamPlayer.beard, userSteamPlayer.skin, userSteamPlayer.color, userSteamPlayer.markerColor,
                userSteamPlayer.hand, userSteamPlayer.shirtItem, userSteamPlayer.pantsItem, userSteamPlayer.hatItem,
                userSteamPlayer.backpackItem, userSteamPlayer.vestItem, userSteamPlayer.maskItem,
                userSteamPlayer.glassesItem, userSteamPlayer.skinItems, userSteamPlayer.skinTags,
                userSteamPlayer.skinDynamicProps, userSteamPlayer.skillset, userSteamPlayer.language,
                userSteamPlayer.lobbyID);

            var playerDummy = new DummyUser(
                (UnturnedUserProvider)(m_UserManager.UserProviders.FirstOrDefault(c => c is UnturnedUserProvider)
                                       ?? throw new InvalidOperationException()),
                m_UserDataStore, Provider.clients.Last(), m_LoggerFactory, StringLocalizer,
                options.DisableSimulations, owners);

            PostAddDummy(playerDummy);

            return playerDummy;
        }

        public Task<DummyUser?> AddDummyByParameters(CSteamID id, HashSet<CSteamID> owners,
            ConfigurationSettings settings)
        {
            throw new NotImplementedException();
        }

        private void PreAddDummy(SteamPlayerID dummy)
        {
            var config = Configuration.Get<Configuration>();
            var events = config.Events ?? throw new ArgumentException(nameof(config.Events));

            if (events.CallOnCheckValidWithExplanation)
            {
                var isValid = true;
                var explanation = string.Empty;
                try
                {
                    Provider.onCheckValidWithExplanation(new()
                    {
                        m_SteamID = dummy.steamID,
                        m_eAuthSessionResponse = EAuthSessionResponse.k_EAuthSessionResponseOK,
                        m_OwnerSteamID = dummy.steamID
                    }, ref isValid, ref explanation);
                }
                catch (Exception e)
                {
                    m_Logger.LogError(e, "Plugin raised an exception from onCheckValidWithExplanation");
                }

                if (!isValid)
                {
                    Provider.pending.RemoveAt(Provider.pending.Count - 1);
                    throw new DummyCanceledSpawnException(
                        $"Plugin reject connection a dummy({dummy.steamID}). Reason: {explanation}");
                }
            }

            if (!config.Events.CallOnCheckBanStatusWithHwid)
            {
                return;
            }

            m_TransportConnection.TryGetIPv4Address(out var ip);
            var isBanned = false;
            var banReason = string.Empty;
            var banRemainingDuration = 0U;
            if (SteamBlacklist.checkBanned(dummy.steamID, ip, out var steamBlacklistID))
            {
                isBanned = true;
                banReason = steamBlacklistID!.reason;
                banRemainingDuration = steamBlacklistID.getTime();
            }

            try
            {
                Provider.onCheckBanStatusWithHWID?.Invoke(dummy, ip, ref isBanned, ref banReason,
                    ref banRemainingDuration);
            }
            catch (Exception e)
            {
                m_Logger.LogError(e, "Plugin raised an exception from onCheckValidWithExplanation: ");
            }

            if (!isBanned)
            {
                return;
            }
            
            Provider.pending.RemoveAt(Provider.pending.Count - 1);
            throw new DummyCanceledSpawnException(
                $"Dummy {dummy.steamID} is banned! Ban reason: {banReason}, duration: {banRemainingDuration}");
        }

        private void PostAddDummy(DummyUser playerDummy)
        {
            var configuration = Configuration.Get<Configuration>();
            var options = configuration.Options ?? throw new ArgumentException(nameof(configuration.Options));
            var fun = configuration.Fun ?? throw new ArgumentException(nameof(configuration.Fun));
            var kickTimer = options.KickDummyAfterSeconds;
            
            if (kickTimer > 0)
            {
                AsyncHelper.Schedule("Kick a dummy timer",
                    () => KickTimerTask(playerDummy.SteamID.m_SteamID, kickTimer));
            }

            UniTask.Run(() => RemoveRigidbody(playerDummy));
            if (fun.AlwaysRotate)
            {
                UniTask.Run(() => RotateDummy(playerDummy, fun.RotateYaw));
            }

            m_Dummies.Add(playerDummy);
        }

        private async UniTask RemoveRigidbody(DummyUser player)
        {
            await UniTask.SwitchToMainThread();
            await UniTask.DelayFrame(1);

            var movement = player.Player.Player.movement;
            var r = movement.gameObject.GetComponent<Rigidbody>();
            Object.Destroy(r);
        }

        private async UniTask RotateDummy(DummyUser player, float rotateYaw)
        {
            while (!m_IsDisposing)
            {
                await UniTask.Delay(1);
                player.Simulation.Yaw += rotateYaw;
            }
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

        public Task<DummyUser?> GetPlayerDummyAsync(ulong id)
        {
            return Task.FromResult<DummyUser?>(Dummies.FirstOrDefault(p => p.SteamID.m_SteamID == id));
        }

        public ValueTask DisposeAsync()
        {
            if (m_IsDisposing)
            {
                return new(Task.CompletedTask);
            }

            m_IsDisposing = true;
            
            return new(ClearDummiesAsync());
        }
    }
}
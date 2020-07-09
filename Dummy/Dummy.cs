using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Patches;
using EvolutionPlugins.Dummy.Providers;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

[assembly: PluginMetadata("EvolutionPlugins.Dummy", Author = "DiFFoZ", DisplayName = "Dummy")]

namespace EvolutionPlugins.Dummy
{
    public class Dummy : OpenModUnturnedPlugin
    {
        private const string _HarmonyId = "evo.diffoz.dummy";

        private readonly Harmony m_Harmony;
        private readonly ILogger m_Logger;
        private readonly IConfiguration m_Configuration;
        private readonly IUserProvider m_UserProvider;
        private readonly IDummyProvider m_DummyProvider;

        public Dummy(IServiceProvider serviceProvider, ILogger<Dummy> logger, IUserManager userManager, IDummyProvider dummyProvider) : base(serviceProvider)
        {
            m_Harmony = new Harmony(_HarmonyId);
            m_Logger = logger;
            m_Configuration = Configuration;
            m_UserProvider = userManager.UserProviders.FirstOrDefault(x => x.SupportsUserType(KnownActorTypes.Player));
            m_DummyProvider = dummyProvider;
        }

        protected override UniTask OnLoadAsync()
        {
            Patch_Provider_receiveServer.onNeedProvider += GiveProvider;
            Patch_Provider_send.OnNeedProvider += GiveProvider;
            Patch_Provider_verifyNextPlayerInQueue.OnNeedProvider += GiveProvider;

            m_Logger.LogInformation("Made with <3 by Evolution Plugins");
            m_Logger.LogInformation("https://github.com/evolutionplugins \\ https://github.com/diffoz");
            m_Logger.LogInformation("Discord: DiFFoZ#6745");

            AsyncHelper.Schedule("Don't auto kick a dummies", DontAutoKickTask);

            m_Harmony.PatchAll();

            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;

            return UniTask.CompletedTask;
        }

        private IDummyProvider GiveProvider()
        {
            return m_DummyProvider;
        }

        protected override UniTask OnUnloadAsync()
        {
            Patch_Provider_receiveServer.onNeedProvider -= GiveProvider;
            Patch_Provider_send.OnNeedProvider -= GiveProvider;
            Patch_Provider_verifyNextPlayerInQueue.OnNeedProvider -= GiveProvider;

            m_Harmony.UnpatchAll(_HarmonyId);

            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
            Provider.onServerDisconnected -= OnServerDisconnected;
            ChatManager.onServerSendingMessage -= OnServerSendingMessage;

            return UniTask.CompletedTask;
        }

        private void OnServerSendingMessage(ref string text, ref Color color, SteamPlayer fromPlayer, SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting)
        {
            if (toPlayer == null)
            {
                return;
            }

            if (!m_DummyProvider.Dummies.ContainsKey(toPlayer.playerID.steamID))
            {
                return;
            }

            var data = m_DummyProvider.Dummies[toPlayer.playerID.steamID];

            foreach (var owner in data.Owners)
            {
                ChatManager.say(owner, $"Dummy {toPlayer.playerID.steamID} got message: {text}", color, true);
            }
        }

        private void OnServerDisconnected(CSteamID steamID)
        {
            AsyncHelper.RunSync(() => m_DummyProvider.RemoveDummyAsync(steamID));
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!m_DummyProvider.Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
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

        private async Task DontAutoKickTask()
        {
            while (IsComponentAlive)
            {
                foreach (var dummy in m_DummyProvider.Dummies)
                {
                    var client = Provider.clients.Find(k => k.playerID.steamID == dummy.Key);
                    if (client == null)
                    {
                        continue;
                    }
                    client.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
                }
                await Task.Delay(5000);
            }
        }
    }
}

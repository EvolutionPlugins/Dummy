using Cysharp.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
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

        public readonly Dictionary<CSteamID, DummyData> Dummies;

        public Dummy(IServiceProvider serviceProvider, ILogger<Dummy> logger) : base(serviceProvider)
        {
            m_Harmony = new Harmony(_HarmonyId);
            m_Logger = logger;
            m_Configuration = Configuration;
            Dummies = new Dictionary<CSteamID, DummyData>();
        }

        protected override async UniTask OnLoadAsync()
        {
            m_Logger.LogInformation("Hello");

            AsyncHelper.Schedule("Don't auto kick a dummies", DontAutoKickTask);

            m_Harmony.PatchAll();

            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
        }

        //protected override void Load()
        //{
        //    Logger.Log("Made with <3 by Evolution Plugins", ConsoleColor.Cyan);
        //    Logger.Log("https://vk.com/evolutionplugins", ConsoleColor.Cyan);
        //    Logger.Log("Discord: DiFFoZ#6745", ConsoleColor.Cyan);

        //    _harmony = new Harmony(HarmonyId);
        //    _harmony.PatchAll();

        //    StartCoroutine(DontAutoKick());

        //    DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
        //    Provider.onServerDisconnected += OnServerDisconnected;
        //    ChatManager.onServerSendingMessage += OnServerSendingMessage; // with old Rocket.Unturned can be problems
        //}

        //protected override void Unload()
        //{
        //    Instance = null;
        //    Config = null;

        //    _harmony.UnpatchAll(HarmonyId);
        //    _harmony = null;

        //    foreach (var dummy in Dummies)
        //    {
        //        Provider.kick(dummy.Key, "");
        //    }
        //    Dummies.Clear();

        //    StopAllCoroutines();

        //    DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
        //    Provider.onServerDisconnected -= OnServerDisconnected;
        //    ChatManager.onServerSendingMessage -= OnServerSendingMessage;
        //}

        private void OnServerSendingMessage(ref string text, ref Color color, SteamPlayer fromPlayer, SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting)
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

            foreach (var owner in data.Owners)
            {
                ChatManager.say(owner, $"Dummy {toPlayer.playerID.steamID} got message: {text}", color, true);
            }
        }

        private void OnServerDisconnected(CSteamID steamID)
        {
            if (Dummies.ContainsKey(steamID))
            {
                Dummies.Remove(steamID);
            }
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
            {
                return;
            }
            float totalTimes = parameters.times;

            if (parameters.respectArmor)
            {
                totalTimes *= DamageTool.getPlayerArmor(parameters.limb, parameters.player);
            }
            if (parameters.applyGlobalArmorMultiplier)
            {
                totalTimes *= Provider.modeConfigData.Players.Armor_Multiplier;
            }
            byte totalDamage = (byte)Mathf.Min(255, parameters.damage * totalTimes);

            ChatManager.say(parameters.killer, $"Amount damage to dummy: {totalDamage}", Color.green);
            shouldAllow = false;
        }

        internal CSteamID GetAvailableID()
        {
            var result = new CSteamID(1);

            while (Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return result;
        }

        private async Task DontAutoKickTask()
        {
            while (IsComponentAlive)
            {
                foreach (var dummy in Dummies)
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

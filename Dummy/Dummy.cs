using HarmonyLib;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Dummy
{
    public class Dummy : RocketPlugin<DummyConfiguration>
    {
        private const string HarmonyId = "evo.diffoz.dummy";

        private Harmony _harmony;

        public static Dummy Instance;
        public DummyConfiguration Config;

        public readonly Dictionary<CSteamID, Coroutine> Dummies = new Dictionary<CSteamID, Coroutine>();

        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;

            Logger.Log("Made with <3 by Evolution Plugins", ConsoleColor.Cyan);
            Logger.Log("https://vk.com/evolutionplugins", ConsoleColor.Cyan);
            Logger.Log("Discord: DiFFoZ#6745", ConsoleColor.Cyan);

            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll();

            StartCoroutine(DontAutoKick());

            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if(!Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
            {
                return;
            }
            float totalTimes = parameters.times;

            if(parameters.respectArmor)
            {
                totalTimes *= DamageTool.getPlayerArmor(parameters.limb, parameters.player);
            }
            if (parameters.applyGlobalArmorMultiplier)
            {
                totalTimes *= Provider.modeConfigData.Players.Armor_Multiplier;
            }
            byte totalDamage = (byte)Mathf.Min(255, parameters.damage * totalTimes);

            ChatManager.say(parameters.killer, $"Amount damage to dummy: {totalDamage}", Color.green);
        }

        protected override void Unload()
        {
            Instance = null;
            Config = null;

            _harmony.UnpatchAll(HarmonyId);
            _harmony = null;

            foreach (var dummy in Dummies)
            {
                Provider.kick(dummy.Key, "");
            }
            Dummies.Clear();

            StopAllCoroutines();
        }

        internal static CSteamID GetAvailableID()
        {
            var result = new CSteamID(1);

            while (Instance.Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return result;
        }

        public Coroutine GetCoroutine(CSteamID id)
        {
            return Config.KickDummyAfterSeconds != 0 ? StartCoroutine(KickTimer(id)) : null;
        }

        private IEnumerator DontAutoKick()
        {
            while(true)
            {
                foreach (var dummy in Dummies)
                {
                    var client = Provider.clients.Find(k => k.playerID.steamID == dummy.Key);
                    client.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
                }
                yield return new WaitForSeconds(5);
            }
        }

        private IEnumerator KickTimer(CSteamID id)
        {
            yield return new WaitForSeconds(Config.KickDummyAfterSeconds);
            Provider.kick(id, "");
        }
    }
}

using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
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

            Logger.Log("Made with <3 by Evolution Plugins", ConsoleColor.Cyan);
            Logger.Log("https://vk.com/evolutionplugins", ConsoleColor.Cyan);
            Logger.Log("Discord: DiFFoZ#6745", ConsoleColor.Cyan);

            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll();
        }

        protected override void Unload()
        {
            Instance = null;

            _harmony.UnpatchAll(HarmonyId);
            _harmony = null;

            foreach (var dummy in Dummies)
            {
                Provider.kick(dummy.Key, "");
                 
                if(dummy.Value != null)
                {
                    StopCoroutine(dummy.Value);
                }
            }
            Dummies.Clear();
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

        public IEnumerator KickTimer(CSteamID id)
        {
            yield return new WaitForSeconds(Config.KickDummyAfterSeconds);
            Provider.kick(id, "");
        }
    }
}

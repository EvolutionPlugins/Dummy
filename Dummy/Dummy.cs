using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Dummy
{
    public class Dummy : RocketPlugin
    {
        private const string HarmonyId = "evo.diffoz.dummy";

        private Harmony _harmony;

        public static Dummy Instance;

        public readonly List<CSteamID> Dummies = new List<CSteamID>();

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
                Provider.kick(dummy, "");
            }
            Dummies.Clear();
        }
    }
}

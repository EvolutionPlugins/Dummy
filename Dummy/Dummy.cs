using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
    public class Dummy : RocketPlugin
    {
        private const string HarmonyId = "evo.diffoz.dummy";

        private Harmony _harmony;

        public static Dummy Instance;

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
        }
    }
}

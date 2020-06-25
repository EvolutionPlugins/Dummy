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
        public static Dummy Instance;

        protected override void Load()
        {
            Instance = this;

            Logger.Log("Made with <3 by Evolution Plugins", ConsoleColor.Cyan);
            Logger.Log("https://vk.com/evolutionplugins", ConsoleColor.Cyan);
            Logger.Log("Discord: ", ConsoleColor.Cyan);
        }

        protected override void Unload()
        {
            
        }
    }
}

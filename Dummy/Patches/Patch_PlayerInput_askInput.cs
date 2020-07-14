using HarmonyLib;

using SDG.Unturned;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy.Patches
{
#if DEBUG
    [HarmonyPatch(typeof(PlayerInput), "askInput")]
    public static class Patch_PlayerInput_askInput
    {
        public static void Prefix(CSteamID steamID)
        {
            if (Dummy.Instance.Dummies.ContainsKey(steamID))
                Console.WriteLine("Get packets from dummy");
        }
    }
#endif
}

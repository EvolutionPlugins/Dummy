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
    [HarmonyPatch(typeof(Provider), "send")]
    public static class Patch_Provider_send
    {
        public static bool Prefix(CSteamID steamID)
        {
            return steamID == CSteamID.Nil;
        }
    }
}

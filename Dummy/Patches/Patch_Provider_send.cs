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
        // Prevent spam about "Failed send packet to ..."
        public static bool Prefix(CSteamID steamID)
        {
            return Dummy.Instance.Dummies.Contains(steamID);
        }
    }
}

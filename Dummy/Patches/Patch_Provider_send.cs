using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System.Linq;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), nameof(Provider.send))]
    public static class Patch_Provider_send
    {
        // Prevent spam about "Failed send packet to ..."
        public static bool Prefix(CSteamID steamID)
        {
            return !Dummy.Instance.Dummies.Any(k => k.Key == steamID);
        }
    }
}

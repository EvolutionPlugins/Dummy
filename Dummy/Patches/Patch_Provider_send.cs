using EvolutionPlugins.Dummy.Providers;
using HarmonyLib;
using SDG.Unturned;
using Steamworks;

namespace EvolutionPlugins.Dummy.Patches
{
    public delegate IDummyProvider NeedProvider();

    [HarmonyPatch(typeof(Provider), "send")]
    public static class Patch_Provider_send
    {
        public static event NeedProvider OnNeedProvider;

        // Prevent spam about "Failed send packet to ..."
        public static bool Prefix(CSteamID steamID)
        {
            return !(OnNeedProvider?.Invoke()?.Dummies.ContainsKey(steamID) ?? false);
        }
    }
}

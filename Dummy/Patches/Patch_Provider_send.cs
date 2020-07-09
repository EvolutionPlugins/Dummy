using EvolutionPlugins.Dummy.Providers;
using HarmonyLib;
using SDG.Unturned;
using Steamworks;

namespace EvolutionPlugins.Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), "send")]
    public static class Patch_Provider_send
    {
        public static IDummyProvider m_DummyProvider;
        // Prevent spam about "Failed send packet to ..."
        public static bool Prefix(CSteamID steamID)
        {
            return false;
        }
    }
}

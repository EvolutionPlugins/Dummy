using EvolutionPlugins.Dummy.Providers;
using HarmonyLib;
using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), "verifyNextPlayerInQueue")]
    public static class Patch_Provider_verifyNextPlayerInQueue
    {
        public static DummyProvider DummyProvider;
        public static bool Prefix()
        {
            if (Provider.pending.Count < 1)
            {
                return false;
            }
            if (Provider.clients.Count - DummyProvider.Dummies.Count >= Provider.maxPlayers)
            {
                return false;
            }
            SteamPending steamPending = Provider.pending[0];
            if (steamPending.hasSentVerifyPacket)
            {
                return false;
            }
            steamPending.sendVerifyPacket();
            return false;
        }
    }
}

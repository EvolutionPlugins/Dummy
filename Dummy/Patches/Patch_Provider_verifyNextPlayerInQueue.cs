using HarmonyLib;
using SDG.Unturned;

namespace EvolutionPlugins.Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), "verifyNextPlayerInQueue")]
    public static class Patch_Provider_verifyNextPlayerInQueue
    {
        internal static NeedDummyProvider OnNeedDummyProvider;

        public static bool Prefix()
        {
            if (Provider.pending.Count < 1)
            {
                return false;
            }
            var dummiesCount = OnNeedDummyProvider?.Invoke()?.Dummies.Count ?? 0;

            if (Provider.clients.Count - dummiesCount >= Provider.maxPlayers)
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

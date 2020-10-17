using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(PlayerTool))]
    public static class Patch_PlayerTool
    {
        internal static event NeedDummyProvider OnNeedDummyProvider;

        [HarmonyPatch("getSteamPlayer", new Type[] { typeof(CSteamID) })]
        [HarmonyPostfix]
        public static void getSteamPlayerBySteamId(CSteamID steamID, ref SteamPlayer __result)
        {
            if (__result != null || OnNeedDummyProvider == null) return;
            var dummy = OnNeedDummyProvider().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy != null)
            {
                __result = dummy.SteamPlayer;
            }
        }

        [HarmonyPatch("getSteamPlayer", new Type[] { typeof(ulong) })]
        [HarmonyPostfix]
        public static void getSteamPlayerByUlong(ulong steamID, ref SteamPlayer __result)
        {
            if (__result != null || OnNeedDummyProvider == null) return;

            var dummy = OnNeedDummyProvider().Dummies.FirstOrDefault(x => x.SteamID == (CSteamID)steamID);
            if (dummy != null)
            {
                __result = dummy.SteamPlayer;
            }
        }

        [HarmonyPatch("getSteamPlayer", new Type[] { typeof(string) })]
        [HarmonyPostfix]
        public static void getSteamPlayerByString(string name, ref SteamPlayer __result)
        {
            if (__result != null || OnNeedDummyProvider == null) return;

            var dummy = OnNeedDummyProvider().Dummies
                .FirstOrDefault(x => NameTool.checkNames(name, x.DisplayName)
                    || NameTool.checkNames(name, x.SteamPlayer.playerID.playerName));
            if (dummy != null)
            {
                __result = dummy.SteamPlayer;
            }
        }
    }
}

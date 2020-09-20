using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;

namespace EvolutionPlugins.Dummy.Patches
{
    [HarmonyPatch(typeof(PlayerTool), "getSteamPlayer", new Type[] { typeof(CSteamID) })]
    public static class Patch_PlayerTool_getSteamPlayer
    {
        internal static event NeedDummyProvider OnNeedDummyProvider;

        [HarmonyPostfix]
        public static void getSteamPlayer(CSteamID steamID, ref SteamPlayer __result)
        {
            if (__result != null || OnNeedDummyProvider == null) return;

            if (OnNeedDummyProvider.Invoke().Dummies.TryGetValue(steamID, out var playerDummy))
            {
                __result = playerDummy.Data.UnturnedUser.Player.SteamPlayer;
            }
        }
    }
}

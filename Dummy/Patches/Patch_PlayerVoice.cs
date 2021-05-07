using System.Linq;
using HarmonyLib;
using SDG.Unturned;

namespace Dummy.Patches
{
    // ReSharper disable InconsistentNaming
    [HarmonyPatch(typeof(PlayerVoice), nameof(PlayerVoice.ReceiveVoiceChatRelay))]
    public static class Patch_PlayerVoice
    {
        internal static event NeedDummyProvider? OnNeedDummy;

        public static void Prefix(PlayerVoice __instance, in ServerInvocationContext context)
        {
            if (__instance.player.life.isDead || !__instance.allowVoiceChat)
            {
                return;
            }

        }
    }
}
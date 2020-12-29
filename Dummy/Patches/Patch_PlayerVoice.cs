using HarmonyLib;
using SDG.Unturned;
using Serilog;
using System.Linq;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(PlayerVoice), nameof(PlayerVoice.askVoiceChat))]
    public static class Patch_PlayerVoice
    {
        internal static event NeedDummyProvider OnNeedDummy;

        public static void Prefix(PlayerVoice __instance, byte[] packet)
        {
            if (__instance.player.life.isDead || !__instance.allowVoiceChat)
            {
                return;
            }
            var dummy = OnNeedDummy?.Invoke().Dummies.FirstOrDefault(c => c.CopyUserVoice != null && c.CopyUserVoice == __instance.player);
            if (dummy == null)
            {
                return;
            }
            // packet.Length.ToString() == 65535
            __instance.channel.decodeVoicePacket(packet, out var size, out var walkie);
            var call = dummy.Player.Player.voice.channel.getCall("askVoiceChat");
            dummy.Player.Player.voice.channel.encodeVoicePacket((byte)call, out var packetSize, out var packet1, packet, (ushort)size, walkie);
            dummy.Player.Player.voice.askVoiceChat(packet1);
        }
    }
}

using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Dummy.Patches
{
#if DEBUG
    [HarmonyPatch(typeof(PlayerInput), "askInput")]
    public static class Patch_PlayerInput_askInput
    {
        public static void Prefix(CSteamID steamID)
        {
            if (steamID.m_SteamID == 1)
                Console.WriteLine($"[{DateTime.Now}] Get packets from dummy");
            else
                Console.WriteLine($"[{DateTime.Now}] Get packets from ME");
        }

        public static void Postfix(CSteamID steamID, Queue<PlayerInputPacket> ___serversidePackets)
        {
            if (steamID.m_SteamID == 1)
                Console.WriteLine("Dummy " + ___serversidePackets.Count);
            else
                Console.WriteLine("ME " + ___serversidePackets.Count);
        }
    }
#endif
}

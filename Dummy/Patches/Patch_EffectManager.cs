using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using Types = SDG.Unturned.Types;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(EffectManager))]
    public static class Patch_EffectManager
    {
        internal static event NeedDummyProvider OnNeedDummy;

        [HarmonyPatch(nameof(EffectManager.sendUIEffectText))]
        [HarmonyPostfix]
        public static void sendUIEffectText(short key, CSteamID steamID, bool reliable, string childName, string text)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got text UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffectText(key, player.playerID.steamID, reliable, childName, text);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectVisibility))]
        [HarmonyPostfix]
        public static void sendUIEffectVisibility(short key, CSteamID steamID, bool reliable, string childName, bool visible)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got visibility UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffectVisibility(key, player.playerID.steamID, reliable, childName, visible);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectImageURL),
            argumentTypes: new Type[] { typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string) })]
        [HarmonyPostfix]
        public static void sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childName, string url)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got imageURL UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffectImageURL(key, player.playerID.steamID, reliable, childName, url);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectImageURL),
            argumentTypes: new Type[] { typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        public static void sendUIEffectImageURL2(short key, CSteamID steamID, bool reliable, string childName, string url, bool shouldCache, bool forceRefresh)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got imageURL UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffectImageURL(key, player.playerID.steamID, reliable, childName, url, shouldCache, forceRefresh);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string) })]
        [HarmonyPostfix]
        public static void sendUIEffect4Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2, string arg3)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (4 args) updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffect(id, key, player.playerID.steamID, reliable, arg0, arg1, arg2, arg3);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(string) })]
        [HarmonyPostfix]
        public static void sendUIEffect3Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (3 args) updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffect(id, key, player.playerID.steamID, reliable, arg0, arg1, arg2);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string) })]
        [HarmonyPostfix]
        public static void sendUIEffect2Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (2 args) updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffect(id, key, player.playerID.steamID, reliable, arg0, arg1);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string) })]
        [HarmonyPostfix]
        public static void sendUIEffect1Arg(ushort id, short key, CSteamID steamID, bool reliable, string arg0)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (1 arg) updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffect(id, key, player.playerID.steamID, reliable, arg0);
            }
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool) })]
        [HarmonyPostfix]
        public static void sendUIEffect0Arg(ushort id, short key, CSteamID steamID, bool reliable)
        {
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return;
            }

            foreach (var owner in dummy.Owners)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (0 arg) updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.sendUIEffect(id, key, player.playerID.steamID, reliable);
            }
        }
    }
}

using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(EffectManager))]
    public static class Patch_EffectManager
    {
        internal static event NeedDummyProvider OnNeedDummy;

        [HarmonyPatch(nameof(EffectManager.askEffectClearByID))]
        [HarmonyPrefix]
        public static bool askEffectClearByID(ushort id, CSteamID steamID)
        {
            if(OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got clear UI (id: {id}). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellEffectClearByID", owner, ESteamPacket.UPDATE_RELIABLE_BUFFER, id);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectText))]
        [HarmonyPrefix]
        public static bool sendUIEffectText(short key, CSteamID steamID, bool reliable, string childName, string text)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got text UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffectText", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, key,
                    childName, text);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectVisibility))]
        [HarmonyPrefix]
        public static bool sendUIEffectVisibility(short key, CSteamID steamID, bool reliable, string childName, bool visible)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got visibility UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffectVisibility", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, key,
                    childName, visible);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectImageURL),
            argumentTypes: new Type[] { typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string) })]
        [HarmonyPrefix]
        public static bool sendUIEffectImageURL(short key, CSteamID steamID, bool reliable, string childName, string url)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got imageURL UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffectImageURL", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, key,
                    childName, url, true, false);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffectImageURL),
            argumentTypes: new Type[] { typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        public static bool sendUIEffectImageURL2(short key, CSteamID steamID, bool reliable, string childName, string url, bool shouldCache, bool forceRefresh)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got imageURL UI updating. Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffectImageURL", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, key,
                    childName, url, shouldCache, forceRefresh);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string) })]
        [HarmonyPrefix]
        public static bool sendUIEffect4Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2, string arg3)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (4 args). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffect4Args", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, id, key,
                    arg0, arg1, arg2, arg3);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string), typeof(string) })]
        [HarmonyPrefix]
        public static bool sendUIEffect3Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1, string arg2)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (3 args). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffect3Args", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, id, key,
                    arg0, arg1, arg2);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string), typeof(string) })]
        [HarmonyPrefix]
        public static bool sendUIEffect2Args(ushort id, short key, CSteamID steamID, bool reliable, string arg0, string arg1)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (2 args). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffect2Args", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, id, key,
                    arg0, arg1);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool), typeof(string) })]
        [HarmonyPrefix]
        public static bool sendUIEffect1Arg(ushort id, short key, CSteamID steamID, bool reliable, string arg0)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (1 arg). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffect1Arg", owner,
                    reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, id, key,
                    arg0);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.sendUIEffect),
            argumentTypes: new Type[] { typeof(ushort), typeof(short), typeof(CSteamID), typeof(bool) })]
        [HarmonyPrefix]
        public static bool sendUIEffect0Arg(ushort id, short key, CSteamID steamID, bool reliable)
        {
            if (OnNeedDummy().Dummies.Any(x => x.SubscribersUI.Contains(steamID)))
            {
                return false;
            }

            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SteamID == steamID);
            if (dummy == null)
            {
                return true;
            }

            foreach (var owner in dummy.SubscribersUI)
            {
                var player = PlayerTool.getSteamPlayer(owner);
                if (player == null)
                {
                    continue;
                }
                ChatManager.serverSendMessage($"Dummy({steamID}) got UI (0 arg). Showing up..", Color.yellow, toPlayer: player);
                EffectManager.instance.channel.send("tellUIEffect", owner,
                     reliable ? ESteamPacket.UPDATE_RELIABLE_BUFFER : ESteamPacket.UPDATE_UNRELIABLE_BUFFER, id, key);
            }
            return true;
        }

        [HarmonyPatch(nameof(EffectManager.tellEffectTextCommitted))]
        [HarmonyPatch(nameof(EffectManager.tellEffectClicked))]
        [HarmonyPrefix]
        public static void InteractingUI(ref CSteamID steamID)
        {
            var altSteamId = steamID;
            var dummy = OnNeedDummy().Dummies.FirstOrDefault(x => x.SubscribersUI.Contains(altSteamId));
            if(dummy != null)
            {
                steamID = dummy.SteamID;
            }
        }
    }
}

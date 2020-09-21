using EvolutionPlugins.Dummy.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Reflection;
using UnityEngine;

namespace EvolutionPlugins.Dummy
{
    public static class Utils
    {
        #region SDG.Unturned methods
        public static void verifyNextPlayerInQueue()
        {
            var dynMethod = typeof(Provider).GetMethod("verifyNextPlayerInQueue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            dynMethod.Invoke(typeof(Provider), Array.Empty<object>());
        }

        public static void loadPlayerSpawn(SteamPlayerID playerID, out Vector3 point, out byte angle, out EPlayerStance stance)
        {
            point = Vector3.zero;
            angle = 0;
            stance = EPlayerStance.STAND;

            var dynMethod = typeof(Provider).GetMethod("loadPlayerSpawn", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var parameters = new object[] { playerID, point, angle, stance };
            dynMethod.Invoke(typeof(Provider), parameters);

            point = (Vector3)parameters[1];
            angle = (byte)parameters[2];
            stance = (EPlayerStance)parameters[3];
        }

        public static SteamPlayer addPlayer(SteamPlayerID playerID, Vector3 point,
            byte angle, bool isPro, bool isAdmin, int channel, byte face, byte hair, byte beard, Color skin, Color color,
            Color markerColor, bool hand, int shirtItem, int pantsItem, int hatItem, int backpackItem, int vestItem,
            int maskItem, int glassesItem, int[] skinItems, string[] skinTags, string[] skinDynamicProps,
            EPlayerSkillset skillset, string language, CSteamID lobbyID)
        {
            Transform transform = null;
            try
            {
                transform = Provider.gameMode.getPlayerGameObject(playerID).transform;
                transform.position = point;
                transform.rotation = Quaternion.Euler(0f, angle * 2f, 0f);
            }
            catch (Exception e)
            {
                UnturnedLog.error("Exception thrown when getting dummy from game mode:");
                UnturnedLog.exception(e);
            }
            SteamPlayer steamPlayer = null;
            try
            {
                steamPlayer = new SteamPlayer(new NullTransportConnection(), playerID, transform, isPro, isAdmin, channel, face,
                    hair, beard, skin, color, markerColor, hand, shirtItem, pantsItem, hatItem, backpackItem, vestItem,
                    maskItem, glassesItem, skinItems, skinTags, skinDynamicProps, skillset, language, lobbyID);
            }
            catch (Exception e2)
            {
                UnturnedLog.error("Exception thrown when adding dummy:");
                UnturnedLog.exception(e2);
            }

            //try
            //{
            //    // todo: add config option
            //    Provider.onEnemyConnected?.Invoke(steamPlayer);
            //}
            //catch (Exception e)
            //{
            //    UnturnedLog.warn("Exception during onEnemyConnected:");
            //    UnturnedLog.exception(e);
            //}
            return steamPlayer;
        }

        public static int allocPlayerChannelId()
        {
            var dynMethod = typeof(Provider).GetMethod("allocPlayerChannelId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return (int)dynMethod.Invoke(typeof(Provider), Array.Empty<object>());
        }

        public static byte[] buildConnectionPacket(SteamPlayer aboutPlayer, SteamPlayer forPlayer, out int size)
        {
            size = 0;

            var dynMethod = typeof(Provider).GetMethod("buildConnectionPacket", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            object[] parametres = new object[] { aboutPlayer, forPlayer, size };
            var packet = (byte[])dynMethod.Invoke(typeof(Provider), parametres);

            size = (int)parametres[2];
            return packet;
        }
        #endregion
    }
}

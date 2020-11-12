using HarmonyLib;
using SDG.Framework.Modules;
using SDG.NetTransport;
using SDG.Provider;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Types = SDG.Unturned.Types;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), "receiveServer")]
    public static class Patch_Provider_receiveServer
    {
        public static bool Prefix(ITransportConnection transportConnection, byte[] packet, int offset)
        {
            var steamPacket = (ESteamPacket)packet[offset];

            if (steamPacket == ESteamPacket.CONNECT)
            {
                if (Provider.findPendingPlayer(transportConnection) != null)
                {
                    Provider.reject(transportConnection, ESteamRejection.ALREADY_PENDING);
                    return false;
                }
                if (Provider.findPlayer(transportConnection) != null)
                {
                    Provider.reject(transportConnection, ESteamRejection.ALREADY_CONNECTED);
                    return false;
                }
                object[] objects = SteamPacker.getObjects(CSteamID.Nil, offset, 0, packet, new Type[]
                {
                    Types.BYTE_TYPE,
                    Types.BYTE_TYPE,
                    Types.STRING_TYPE,
                    Types.STRING_TYPE,
                    Types.BYTE_ARRAY_TYPE,
                    Types.BYTE_ARRAY_TYPE,
                    Types.BYTE_ARRAY_TYPE,
                    Types.BYTE_TYPE,
                    Types.UINT32_TYPE,
                    Types.BOOLEAN_TYPE,
                    Types.SINGLE_TYPE,
                    Types.STRING_TYPE,
                    Types.STEAM_ID_TYPE,
                    Types.BYTE_TYPE,
                    Types.BYTE_TYPE,
                    Types.BYTE_TYPE,
                    Types.COLOR_TYPE,
                    Types.COLOR_TYPE,
                    Types.COLOR_TYPE,
                    Types.BOOLEAN_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_TYPE,
                    Types.UINT64_ARRAY_TYPE,
                    Types.BYTE_TYPE,
                    Types.STRING_TYPE,
                    Types.STRING_TYPE,
                    Types.STEAM_ID_TYPE,
                    Types.UINT32_TYPE,
                    Types.BYTE_ARRAY_TYPE,
                    Types.BYTE_ARRAY_TYPE,
                    Types.STEAM_ID_TYPE
                });
                byte[] array3 = (byte[])objects[33];
                byte[] hash_ = (byte[])objects[34];
                CSteamID steamID = (CSteamID)objects[35];
                if (array3.Length != 20)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
                    return false;
                }
                byte newCharacterID = (byte)objects[1];
                if (!Provider.modeConfigData.Players.Allow_Per_Character_Saves)
                {
                    newCharacterID = 0;
                }
                SteamPlayerID steamPlayerID = new SteamPlayerID(steamID, newCharacterID, (string)objects[2],
                    (string)objects[3], (string)objects[11], (CSteamID)objects[12], array3);
                if ((uint)objects[8] != Provider.APP_VERSION_PACKED)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_VERSION, Provider.APP_VERSION);
                    return false;
                }
                if ((uint)objects[32] != Level.packedVersion)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_LEVEL_VERSION, Level.version);
                    return false;
                }
                if (steamPlayerID.playerName.Length < 2)
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_SHORT);
                    return false;
                }
                if (steamPlayerID.characterName.Length < 2)
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_SHORT);
                    return false;
                }
                if (steamPlayerID.playerName.Length > 32)
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_LONG);
                    return false;
                }
                if (steamPlayerID.characterName.Length > 32)
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_LONG);
                    return false;
                }

                if (long.TryParse(steamPlayerID.playerName, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                    || double.TryParse(steamPlayerID.playerName, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_NUMBER);
                    return false;
                }

                if (long.TryParse(steamPlayerID.characterName, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                    || double.TryParse(steamPlayerID.characterName, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_NUMBER);
                    return false;
                }
                if (Provider.filterName)
                {
                    if (!NameTool.isValid(steamPlayerID.playerName))
                    {
                        Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
                        return false;
                    }
                    if (!NameTool.isValid(steamPlayerID.characterName))
                    {
                        Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
                        return false;
                    }
                }
                if (NameTool.containsRichText(steamPlayerID.playerName))
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_PLAYER_INVALID);
                    return false;
                }
                if (NameTool.containsRichText(steamPlayerID.characterName))
                {
                    Provider.reject(transportConnection, ESteamRejection.NAME_CHARACTER_INVALID);
                    return false;
                }
                transportConnection.TryGetIPv4Address(out var remoteIP);
                Utils.checkBanStatus(steamPlayerID, remoteIP, out bool flag3, out string object_, out uint num5);
                if (flag3)
                {
                    byte[] bytes3 = SteamPacker.getBytes(0, out int size4, 9, object_, num5);
                    Provider.sendToClient(transportConnection, ESteamPacket.BANNED, bytes3, size4);
                    return false;
                }
                bool flag4 = SteamWhitelist.checkWhitelisted(steamID);
                if (Provider.isWhitelisted && !flag4)
                {
                    Provider.reject(transportConnection, ESteamRejection.WHITELISTED);
                    return false;
                }
                if (Provider.clients.Count - Dummy.Instance.Dummies.Count + 1 > Provider.maxPlayers && Provider.pending.Count + 1 > Provider.queueSize)
                {
                    Provider.reject(transportConnection, ESteamRejection.SERVER_FULL);
                    return false;
                }
                byte[] array4 = (byte[])objects[4];
                if (array4.Length != 20)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
                    return false;
                }
                byte[] array5 = (byte[])objects[5];
                if (array5.Length != 20)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
                    return false;
                }
                byte[] array6 = (byte[])objects[6];
                if (array6.Length != 20)
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
                    return false;
                }
                if (Provider.configData.Server.Validate_EconInfo_Hash && !Hash.verifyHash(hash_, TempSteamworksEconomy.econInfoHash))
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ECON);
                    return false;
                }
                string text = (string)objects[29];
                ModuleDependency[] array7;
                if (string.IsNullOrEmpty(text))
                {
                    array7 = Array.Empty<ModuleDependency>();
                }
                else
                {
                    string[] array8 = text.Split(new char[]
                    {
                    ';'
                    });
                    array7 = new ModuleDependency[array8.Length];
                    for (int n = 0; n < array7.Length; n++)
                    {
                        string[] array9 = array8[n].Split(new char[]
                        {
                        ','
                        });
                        if (array9.Length == 2)
                        {
                            array7[n] = new ModuleDependency
                            {
                                Name = array9[0]
                            };
                            uint.TryParse(array9[1], NumberStyles.Any, CultureInfo.InvariantCulture, out array7[n].Version_Internal);
                        }
                    }
                }
                var moduleList = new List<Module>();
                ModuleHook.getRequiredModules(moduleList);
                bool flag5 = true;
                for (int num6 = 0; num6 < array7.Length; num6++)
                {
                    bool flag6 = false;
                    if (array7[num6] != null)
                    {
                        for (int num7 = 0; num7 < moduleList.Count; num7++)
                        {
                            if (moduleList[num7]?.config != null && moduleList[num7].config.Name == array7[num6].Name && moduleList[num7].config.Version_Internal >= array7[num6].Version_Internal)
                            {
                                flag6 = true;
                                break;
                            }
                        }
                    }
                    if (!flag6)
                    {
                        flag5 = false;
                        break;
                    }
                }
                if (!flag5)
                {
                    Provider.reject(transportConnection, ESteamRejection.CLIENT_MODULE_DESYNC);
                    return false;
                }
                bool flag7 = true;
                for (int num8 = 0; num8 < moduleList.Count; num8++)
                {
                    bool flag8 = false;
                    if (moduleList[num8]?.config != null)
                    {
                        for (int num9 = 0; num9 < array7.Length; num9++)
                        {
                            if (array7[num9] != null && array7[num9].Name == moduleList[num8].config.Name && array7[num9].Version_Internal >= moduleList[num8].config.Version_Internal)
                            {
                                flag8 = true;
                                break;
                            }
                        }
                    }
                    if (!flag8)
                    {
                        flag7 = false;
                        break;
                    }
                }
                if (!flag7)
                {
                    Provider.reject(transportConnection, ESteamRejection.SERVER_MODULE_DESYNC);
                    return false;
                }
                if (!string.IsNullOrEmpty(Provider.serverPassword) && !Hash.verifyHash(array4, Provider.serverPasswordHash))
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_PASSWORD);
                    return false;
                }
                if (!Hash.verifyHash(array5, Level.hash))
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_LEVEL);
                    return false;
                }
                if (!ReadWrite.appIn(array6, (byte)objects[7]))
                {
                    Provider.reject(transportConnection, ESteamRejection.WRONG_HASH_ASSEMBLY);
                    return false;
                }
                if ((float)objects[10] >= Provider.configData.Server.Max_Ping_Milliseconds / 1000f)
                {
                    Provider.reject(transportConnection, ESteamRejection.PING);
                    return false;
                }
                Utils.notifyClientPending(steamID);
                SteamPending item = new SteamPending(
                    transportConnection, steamPlayerID, (bool)objects[9], (byte)objects[13], (byte)objects[14],
                    (byte)objects[15], (Color)objects[16], (Color)objects[17], (Color)objects[18], (bool)objects[19],
                    (ulong)objects[20], (ulong)objects[21], (ulong)objects[22], (ulong)objects[23], (ulong)objects[24],
                    (ulong)objects[25], (ulong)objects[26], (ulong[])objects[27], (EPlayerSkillset)(byte)objects[28],
                    (string)objects[30], (CSteamID)objects[31]);
                if (Provider.isWhitelisted || !flag4)
                {
                    Provider.pending.Add(item);
                    if (Provider.pending.Count == 1)
                    {
                        Utils.verifyNextPlayerInQueue();
                    }
                    return false;
                }
                if (Provider.pending.Count == 0)
                {
                    Provider.pending.Add(item);
                    Utils.verifyNextPlayerInQueue();
                    return false;
                }
                Provider.pending.Insert(1, item);
                return false;
            }
            return true;
        }
    }
}

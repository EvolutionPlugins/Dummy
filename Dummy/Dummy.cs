﻿using Dummy.Configurations;

using HarmonyLib;

using Rocket.Core.Plugins;

using SDG.Unturned;

using Steamworks;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Logger = Rocket.Core.Logging.Logger;

namespace Dummy
{
    public class Dummy : RocketPlugin<DummyConfiguration>
    {
        private const string HarmonyId = "evo.diffoz.dummy";

        private Harmony _harmony;

        public static Dummy Instance;
        public DummyConfiguration Config;

        public readonly Dictionary<CSteamID, DummyData> Dummies = new Dictionary<CSteamID, DummyData>();

        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;

            Logger.Log("Made with <3 by Evolution Plugins", ConsoleColor.Cyan);
            Logger.Log("https://vk.com/evolutionplugins", ConsoleColor.Cyan);
            Logger.Log("Discord: DiFFoZ#6745", ConsoleColor.Cyan);

            _harmony = new Harmony(HarmonyId);
            _harmony.PatchAll();

            InvokeRepeating("DontAutoKick", 5f, 5f);

            DamageTool.damagePlayerRequested += DamageTool_damagePlayerRequested;
            Provider.onServerDisconnected += OnServerDisconnected;
            ChatManager.onServerSendingMessage += OnServerSendingMessage;
        }

        protected override void Unload()
        {
            Instance = null;
            Config = null;

            _harmony.UnpatchAll(HarmonyId);
            _harmony = null;

            foreach (var dummy in Dummies)
            {
                Provider.kick(dummy.Key, "");
            }
            Dummies.Clear();

            StopAllCoroutines();
            CancelInvoke("DontAutoKick");

            DamageTool.damagePlayerRequested -= DamageTool_damagePlayerRequested;
            Provider.onServerDisconnected -= OnServerDisconnected;
            ChatManager.onServerSendingMessage -= OnServerSendingMessage;
        }

        #region Events
        private void OnServerSendingMessage(ref string text, ref Color color, SteamPlayer fromPlayer, SteamPlayer toPlayer, EChatMode mode, ref string iconURL, ref bool useRichTextFormatting)
        {
            if (toPlayer == null)
            {
                return;
            }

            if (!Dummies.ContainsKey(toPlayer.playerID.steamID))
            {
                return;
            }

            var data = Dummies[toPlayer.playerID.steamID];

            foreach (var owner in data.Owners)
            {
                ChatManager.say(owner, $"Dummy {toPlayer.playerID.steamID} got message: {text}", color, true);
            }
        }

        private void OnServerDisconnected(CSteamID steamID)
        {
            if (Dummies.ContainsKey(steamID))
            {
                var coroutine = Dummies[steamID].Coroutine;
                if (coroutine != null)
                    StopCoroutine(coroutine);

                Dummies.Remove(steamID);
            }
        }

        private void DamageTool_damagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (!Dummies.ContainsKey(parameters.player.channel.owner.playerID.steamID))
            {
                return;
            }
            float totalTimes = parameters.times;

            if (parameters.respectArmor)
            {
                totalTimes *= DamageTool.getPlayerArmor(parameters.limb, parameters.player);
            }
            if (parameters.applyGlobalArmorMultiplier)
            {
                totalTimes *= Provider.modeConfigData.Players.Armor_Multiplier;
            }
            byte totalDamage = (byte)Mathf.Min(255, parameters.damage * totalTimes);

            ChatManager.say(parameters.killer, $"Amount damage to dummy: {totalDamage}", Color.green);
            shouldAllow = false;
        }
        #endregion

        private void FixedUpdate()
        {
            foreach (var item in Dummies)
            {
                var data = item.Value;
                if (data.player == null)
                {
                    continue;
                }

                if (data.count % PlayerInput.SAMPLES == 0)
                {
                    data.tick = Time.realtimeSinceStartup;

                    data.player.input.keys[0] = data.player.movement.jump;
                    data.player.input.keys[1] = data.player.equipment.primary;
                    data.player.input.keys[2] = data.player.equipment.secondary;
                    data.player.input.keys[3] = data.player.stance.crouch;
                    data.player.input.keys[4] = data.player.stance.prone;
                    data.player.input.keys[5] = data.player.stance.sprint;
                    data.player.input.keys[6] = data.player.animator.leanLeft;
                    data.player.input.keys[7] = data.player.animator.leanRight;
                    data.player.input.keys[8] = false;

                    for (var i = 0; i < ControlsSettings.NUM_PLUGIN_KEYS; i++)
                    {
                        var num = data.player.input.keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + i;
                        data.player.input.keys[num] = false;
                    }

                    data.analog = (byte)(data.player.movement.horizontal << 4 | data.player.movement.vertical);
                    data.pitch = data.player.look.pitch;
                    data.yaw = data.player.look.yaw;
                    data.sequence++;

                    if (data.player.stance.stance == EPlayerStance.DRIVING)
                    {
                        data.playerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        data.playerInputPackets.Add(new WalkingPlayerInputPacket());
                    }
                    var playerInputPacket = data.playerInputPackets[data.playerInputPackets.Count - 1];
                    playerInputPacket.sequence = data.sequence;
                    playerInputPacket.recov = data.recov;

                    data.buffer += PlayerInput.SAMPLES;
                    data.simulation += 1;

                    if (data.consumed < data.buffer)
                    {
                        data.consumed += 1;
                        data.clock += 1;
                    }

                    if (data.consumed == data.buffer && data.playerInputPackets.Count > 0)
                    {
                        ushort num2 = 0;
                        byte b = 0;
                        while (b < data.player.input.keys.Length)
                        {
                            if (data.player.input.keys[b])
                            {
                                num2 |= data.flags[b];
                            }
                            b += 1;
                        }

                        var playerInputPacket2 = data.playerInputPackets[data.playerInputPackets.Count - 1];
                        playerInputPacket2.keys = num2;
                        if (playerInputPacket2 is DrivingPlayerInputPacket)
                        {
                            var drivingPlayerInputPacket = playerInputPacket2 as DrivingPlayerInputPacket;
                            var vehicle = data.player.movement.getVehicle();

                            if (vehicle != null)
                            {
                                var transform = vehicle.transform;
                                if (vehicle.asset.engine == EEngine.TRAIN)
                                {
                                    drivingPlayerInputPacket.position = new Vector3(vehicle.roadPosition, 0f, 0f);
                                }
                                else
                                {
                                    drivingPlayerInputPacket.position = transform.position;
                                }
                                drivingPlayerInputPacket.angle_x = MeasurementTool.angleToByte2(transform.rotation.eulerAngles.x);
                                drivingPlayerInputPacket.angle_y = MeasurementTool.angleToByte2(transform.rotation.eulerAngles.y);
                                drivingPlayerInputPacket.angle_z = MeasurementTool.angleToByte2(transform.rotation.eulerAngles.z);
                                drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.physicsSpeed = (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.turn = (byte)(vehicle.turn + 1);
                            }
                        }
                        else
                        {
                            var walkingPlayerInputPacket = playerInputPacket2 as WalkingPlayerInputPacket;

                            walkingPlayerInputPacket.analog = data.analog;
                            walkingPlayerInputPacket.position = data.player.transform.localPosition;
                            walkingPlayerInputPacket.yaw = data.yaw;
                            walkingPlayerInputPacket.pitch = data.pitch;
                        }

                        data.player.input.channel.openWrite();
                        if (data.playerInputPackets.Count > 24)
                        {
                            UnturnedLog.warn("Discarding old unacknowledged input packets ({0}/{1})", new object[]
                            {
                                data.playerInputPackets.Count,
                                24
                            });

                            while (data.playerInputPackets.Count > 24)
                            {
                                data.playerInputPackets.RemoveAt(0);
                            }
                        }
                        data.player.input.channel.write((byte)data.playerInputPackets.Count);

                        foreach (var playerInputPacket3 in data.playerInputPackets)
                        {
                            if (playerInputPacket3 is DrivingPlayerInputPacket)
                            {
                                data.player.input.channel.write(1);
                            }
                            else
                            {
                                data.player.input.channel.write(0);
                            }
                            playerInputPacket3.write(data.player.input.channel);
                        }

                        data.player.input.channel.closeWrite("askInput", ESteamCall.SERVER, ESteamPacket.UPDATE_UNRELIABLE_CHUNK_INSTANT);
                    }
                    data.count += 1U;
                }
            }
        }

        internal static CSteamID GetAvailableID()
        {
            var result = new CSteamID(1);

            while (Instance.Dummies.ContainsKey(result))
            {
                result.m_SteamID++;
            }
            return result;
        }

        public Coroutine GetCoroutine(CSteamID id)
        {
            return Config.KickDummyAfterSeconds != 0 ? StartCoroutine(KickTimer(id)) : null;
        }

        private void DontAutoKick()
        {
            foreach (var dummy in Dummies)
            {
                var client = dummy.Value.player;
                if (client == null)
                {
                    continue;
                }
                client.channel.owner.timeLastPacketWasReceivedFromClient = Time.realtimeSinceStartup;
            }
        }

        private IEnumerator KickTimer(CSteamID id)
        {
            yield return new WaitForSeconds(Config.KickDummyAfterSeconds);
            CommandWindow.Log($"Kicking a dummy {id}");
            Provider.kick(id, "");
        }
    }
}

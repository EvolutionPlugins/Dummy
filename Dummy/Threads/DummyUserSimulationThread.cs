﻿using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Dummy.Threads
{
    public class DummyUserSimulationThread
    {
        private static readonly FieldInfo s_ServerSidePacketsField = typeof(PlayerInput).GetField("serversidePackets",
            BindingFlags.NonPublic | BindingFlags.Instance);

        public byte Analog { get; private set; }
        public uint Count { get; private set; }
        public float Tick { get; private set; }
        public uint Buffer { get; private set; }
        public uint Simulation { get; private set; }
        public int Sequence { get; private set; }
        public int Recov { get; }
        public uint Consumed { get; private set; }
        public uint Clock { get; private set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public ushort[] Flags { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public List<PlayerInputPacket> PlayerInputPackets { get; }
        public bool Enabled { get; set; }

        private readonly DummyUser m_PlayerDummy;
        private Player Player => m_PlayerDummy.Player.Player;

        public DummyUserSimulationThread(DummyUser playerDummy)
        {
            m_PlayerDummy = playerDummy;
            Analog = 0;
            Count = 0;
            Tick = Time.realtimeSinceStartup;
            Buffer = 0;
            Simulation = 0;
            Sequence = -1;
            Recov = -1;
            Consumed = 0;
            Clock = 0;
            Yaw = 0;
            Pitch = 0;
            PlayerInputPackets = new List<PlayerInputPacket>();
            Flags = new ushort[9 + ControlsSettings.NUM_PLUGIN_KEYS];
            for (byte b = 0; b < 9 + ControlsSettings.NUM_PLUGIN_KEYS; b++)
            {
                Flags[b] = (ushort)(1 << b);
            }
        }

        public async UniTask StartSimulation()
        {
            await UniTask.Delay(1000); // waiting
            while (Enabled)
            {
                if (Count % PlayerInput.SAMPLES == 0)
                {
                    Tick = Time.realtimeSinceStartup;

                    Player.input.keys[0] = Player.movement.jump;
                    Player.input.keys[1] = Player.equipment.primary;
                    Player.input.keys[2] = Player.equipment.secondary;
                    Player.input.keys[3] = Player.stance.crouch;
                    Player.input.keys[4] = Player.stance.prone;
                    Player.input.keys[5] = Player.stance.sprint;
                    Player.input.keys[6] = Player.animator.leanLeft;
                    Player.input.keys[7] = Player.animator.leanRight;
                    Player.input.keys[8] = false;

                    for (var i = 0; i < ControlsSettings.NUM_PLUGIN_KEYS; i++)
                    {
                        var num = Player.input.keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + i;
                        Player.input.keys[num] = false;
                    }

                    Analog = (byte)(Player.movement.horizontal << 4 | Player.movement.vertical);
                    Pitch = Player.look.pitch;
                    Yaw = Player.look.yaw;
                    Sequence++;

                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        PlayerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        PlayerInputPackets.Add(new WalkingPlayerInputPacket());
                    }
                    var playerInputPacket = PlayerInputPackets[PlayerInputPackets.Count - 1];
                    playerInputPacket.sequence = Sequence;
                    playerInputPacket.recov = Recov;

                    Buffer += PlayerInput.SAMPLES;
                    Simulation++;
                }
                if (Consumed < Buffer)
                {
                    Consumed++;
                    Clock++;
                }
                if (Consumed == Buffer && PlayerInputPackets.Count > 0)
                {
                    ushort num2 = 0;
                    for (byte b = 0; b < Player.input.keys.Length; b++)
                    {
                        if (Player.input.keys[b])
                        {
                            num2 |= Flags[b];
                        }
                    }

                    var playerInputPacket2 = PlayerInputPackets[PlayerInputPackets.Count - 1];
                    playerInputPacket2.keys = num2;
                    if (playerInputPacket2 is DrivingPlayerInputPacket)
                    {
                        var drivingPlayerInputPacket = playerInputPacket2 as DrivingPlayerInputPacket;
                        var vehicle = Player.movement.getVehicle();

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

                        walkingPlayerInputPacket.analog = Analog;
                        walkingPlayerInputPacket.position = Player.transform.localPosition;
                        walkingPlayerInputPacket.yaw = Yaw;
                        walkingPlayerInputPacket.pitch = Pitch;
                    }

                    await UniTask.SwitchToMainThread();
                    //_playerDummy.Data.UnturnedUser.Player.Player.input.channel.openWrite();
                    if (PlayerInputPackets.Count > 24)
                    {
                        while (PlayerInputPackets.Count > 24)
                        {
                            PlayerInputPackets.RemoveAt(0);
                        }
                    }
                    //_playerDummy.Data.UnturnedUser.Player.Player.input.channel.write((byte)playerInputPackets.Count);
                    // todo: remake
                    var queue = (Queue<PlayerInputPacket>)s_ServerSidePacketsField.GetValue(Player.input);
                    foreach (var playerInputPacket3 in PlayerInputPackets)
                    {
                        queue.Enqueue(playerInputPacket3);
                    }
                    s_ServerSidePacketsField.SetValue(Player.input, queue);
                    await UniTask.SwitchToTaskPool();
                }
                Count++;
            }
        }
    }
}
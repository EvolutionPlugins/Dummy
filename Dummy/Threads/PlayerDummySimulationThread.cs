using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Threads
{
    public class PlayerDummySimulationThread
    {
        private readonly FieldInfo _serverSidePacketsField;

        public byte analog;
        public uint count;
        public float tick;
        public uint buffer;
        public uint simulation;
        public int sequence;
        public int recov;
        public uint consumed;
        public uint clock;
        public ushort[] flags;
        public float yaw;
        public float pitch;
        public List<PlayerInputPacket> playerInputPackets;

        public bool Enabled { get; set; }

        private readonly PlayerDummy _playerDummy;
        private Player Player => _playerDummy.Data.UnturnedUser.Player.Player;

        public PlayerDummySimulationThread(PlayerDummy playerDummy)
        {
            _serverSidePacketsField = typeof(PlayerInput).GetField("serversidePackets", BindingFlags.NonPublic | BindingFlags.Instance);
            _playerDummy = playerDummy;
            analog = 0;
            count = 0;
            tick = Time.realtimeSinceStartup;
            buffer = 0;
            simulation = 0;
            sequence = -1;
            recov = -1;
            consumed = 0;
            clock = 0;
            yaw = 0;
            pitch = 0;
            playerInputPackets = new List<PlayerInputPacket>();
            flags = new ushort[9 + ControlsSettings.NUM_PLUGIN_KEYS];
            for (byte b = 0; b < 9 + ControlsSettings.NUM_PLUGIN_KEYS; b++)
            {
                flags[b] = (ushort)(1 << b);
            }
        }

        public async UniTask StartSimulation()
        {
            await UniTask.Delay(1000); // waiting
            while (Enabled)
            {
                if (count % PlayerInput.SAMPLES == 0)
                {
                    tick = Time.realtimeSinceStartup;

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

                    analog = (byte)(Player.movement.horizontal << 4 | Player.movement.vertical);
                    pitch = Player.look.pitch;
                    yaw = Player.look.yaw;
                    sequence++;

                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        playerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        playerInputPackets.Add(new WalkingPlayerInputPacket());
                    }
                    var playerInputPacket = playerInputPackets[playerInputPackets.Count - 1];
                    playerInputPacket.sequence = sequence;
                    playerInputPacket.recov = recov;

                    buffer += PlayerInput.SAMPLES;
                    simulation++;
                }
                if (consumed < buffer)
                {
                    consumed++;
                    clock++;
                }
                if (consumed == buffer && playerInputPackets.Count > 0)
                {
                    ushort num2 = 0;
                    for (byte b = 0; b < Player.input.keys.Length; b++)
                    {
                        if (Player.input.keys[b])
                        {
                            num2 |= flags[b];
                        }
                    }

                    var playerInputPacket2 = playerInputPackets[playerInputPackets.Count - 1];
                    playerInputPacket2.keys = num2;
                    if (playerInputPacket2 is DrivingPlayerInputPacket)
                    {
                        var drivingPlayerInputPacket = playerInputPacket2 as DrivingPlayerInputPacket;
                        var vehicle = _playerDummy.Data.UnturnedUser.Player.Player.movement.getVehicle();

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

                        walkingPlayerInputPacket.analog = analog;
                        walkingPlayerInputPacket.position = _playerDummy.Data.UnturnedUser.Player.Player.transform.localPosition;
                        walkingPlayerInputPacket.yaw = yaw;
                        walkingPlayerInputPacket.pitch = pitch;
                    }

                    await UniTask.SwitchToMainThread();
                    //_playerDummy.Data.UnturnedUser.Player.Player.input.channel.openWrite();
                    if (playerInputPackets.Count > 24)
                    {
                        while (playerInputPackets.Count > 24)
                        {
                            playerInputPackets.RemoveAt(0);
                        }
                    }
                    //_playerDummy.Data.UnturnedUser.Player.Player.input.channel.write((byte)playerInputPackets.Count);
                    // todo: remake
                    var queue = (Queue<PlayerInputPacket>)_serverSidePacketsField.GetValue(Player.input);
                    foreach (var playerInputPacket3 in playerInputPackets)
                    {
                        queue.Enqueue(playerInputPacket3);
                    }
                    _serverSidePacketsField.SetValue(Player.input, queue);
                    await UniTask.SwitchToTaskPool();
                }
                count++;
            }
        }
    }
}
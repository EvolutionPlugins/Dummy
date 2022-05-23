using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Vector3 = UnityEngine.Vector3;

namespace Dummy.Threads
{
    public class DummyUserSimulationThread
    {
        private static readonly FieldInfo s_ServerSidePacketsField = typeof(PlayerInput).GetField("serversidePackets",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        public byte Analog { get; private set; }
        public uint Count { get; private set; }
        public float Tick { get; private set; }
        public uint Buffer { get; private set; }
        public uint Simulation { get; private set; }
        public int Sequence { get; private set; }
        public int Recov { get; }
        public uint Consumed { get; private set; }
        public uint Clock { get; private set; }
        public ushort[] Flags { get; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public List<PlayerInputPacket> PlayerInputPackets { get; }
        public bool Enabled { get; set; }

        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;
        private Vector3 m_Velocity;

        private Player Player => m_PlayerDummy.Player.Player;

        public DummyUserSimulationThread(DummyUser playerDummy, ILogger logger)
        {
            m_PlayerDummy = playerDummy;
            m_Logger = logger;
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

        public async UniTask Start()
        {
            await UniTask.Delay(1000); // waiting
            var queue = (Queue<PlayerInputPacket>)s_ServerSidePacketsField.GetValue(Player.input);
            while (Enabled)
            {
                await UniTask.WaitForFixedUpdate();
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
                        Player.input.keys[num] = false; // todo
                    }

                    Analog = (byte)(Player.movement.horizontal << 4 | Player.movement.vertical);
                    Pitch = Player.look.pitch;
                    Yaw = Player.look.yaw;
                    Sequence++;

                    var input_x = Player.movement.horizontal - 1;
                    var input_y = Player.movement.vertical - 1;
                    var jump = Player.movement.jump;
                    var sprint = Player.stance.sprint;
                    var look = Player.look;

                    /*Player.movement.simulate(Simulation, 0, input_x, input_y, look.look_x, look.look_y,
                        jump, sprint, PlayerInput.RATE);*/

                    var movement = Player.movement;
                    var flag3 = false;
                    var flag2 = false;
                    if (movement.isGrounded && movement.ground.normal.y > 0)
                    {
                        var num2 = Vector3.Angle(Vector3.up, movement.ground.normal);
                        var num3 = 59f;
                        if (Level.info?.configData?.Max_Walkable_Slope > -0.5f)
                        {
                            num3 = Level.info.configData.Max_Walkable_Slope;
                        }
                        if (num2 > num3)
                        {
                            flag3 = true;
                            var a = Vector3.Cross(Vector3.Cross(Vector3.up, movement.ground.normal), movement.ground.normal);
                            m_Velocity += a * 16f * PlayerInput.RATE;
                            flag2 = true;
                        }
                    }

                    if (!flag3)
                    {
                        var vector = movement.transform.rotation * Vector3.right.normalized * movement.speed;

                        if (movement.isGrounded)
                        {
                            vector = Vector3.Cross(Vector3.Cross(Vector3.up, vector), movement.ground.normal);
                            vector.y = Mathf.Min(vector.y, 0f);

                            // it should also change direction on where is a dummy stand on (exmaple: ice)
                            // if (movement.material)

                            m_Velocity = vector;
                        }
                        else
                        {
                            m_Velocity.y += Physics.gravity.y * ((movement.fall <= 0f) ? movement.totalGravityMultiplier : 1f) * PlayerInput.RATE * 3f;
                            var a2 = (movement.totalGravityMultiplier < 0.99f) ? (Physics.gravity.y * 2f * movement.totalGravityMultiplier) : -100f;
                            m_Velocity.y = Mathf.Max(a2, m_Velocity.y);

                            var horizontalMagnitude = vector.GetHorizontalMagnitude();
                            var horizontal = m_Velocity.GetHorizontal();
                            var horizontalMagnitude2 = m_Velocity.GetHorizontalMagnitude();
                            float maxMagnitude;
                            if (horizontalMagnitude2 > horizontalMagnitude)
                            {
                                var num5 = 2f * Provider.modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier;
                                maxMagnitude = Mathf.Max(horizontalMagnitude, horizontalMagnitude2 - num5 * PlayerInput.RATE);
                            }
                            else
                            {
                                maxMagnitude = horizontalMagnitude;
                            }

                            var a3 = vector * (4f * Provider.modeConfigData.Gameplay.AirStrafing_Acceleration_Multiplier);
                            var vector2 = horizontal + a3 * PlayerInput.RATE;
                            vector2 = vector2.ClampHorizontalMagnitude(maxMagnitude);
                            m_Velocity.x = vector2.x;
                            m_Velocity.z = vector2.z;
                            flag2 = true;
                        }
                    }

                    var previousPosition = movement.transform.position;
                    movement.controller.CheckedMove(m_Velocity * PlayerInput.RATE);

                    if (flag2)
                    {
                        m_Velocity = (movement.transform.position - previousPosition) / PlayerInput.RATE;
                    }

                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        PlayerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        PlayerInputPackets.Add(new WalkingPlayerInputPacket());
                    }

                    var playerInputPacket = PlayerInputPackets[PlayerInputPackets.Count - 1];
                    playerInputPacket.recov = Recov;
                    playerInputPacket.clientSimulationFrameNumber = Simulation;

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
                    if (playerInputPacket2 is DrivingPlayerInputPacket drivingPlayerInputPacket)
                    {
                        var vehicle = Player.movement.getVehicle();

                        if (vehicle != null)
                        {
                            var transform = vehicle.transform;
                            drivingPlayerInputPacket.position = vehicle.asset.engine == EEngine.TRAIN
                                ? new Vector3(vehicle.roadPosition, 0f, 0f)
                                : transform.position;

                            drivingPlayerInputPacket.rotation = transform.rotation;

                            drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                            drivingPlayerInputPacket.physicsSpeed =
                                (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                            drivingPlayerInputPacket.turn = (byte)(vehicle.turn + 1);
                        }
                    }
                    else
                    {
                        var walkingPlayerInputPacket = playerInputPacket2 as WalkingPlayerInputPacket;

                        walkingPlayerInputPacket!.analog = Analog;
                        walkingPlayerInputPacket.clientPosition = Player.transform.position; // before: localposition
                        walkingPlayerInputPacket.yaw = Yaw;
                        walkingPlayerInputPacket.pitch = Pitch;
                    }

                    foreach (var playerInputPacket3 in PlayerInputPackets)
                    {
                        queue.Enqueue(playerInputPacket3);
                    }

                    PlayerInputPackets.Clear();
                }

                Count++;
            }
        }
    }
}
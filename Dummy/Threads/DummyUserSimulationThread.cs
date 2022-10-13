using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction;
using Dummy.Users;
using SDG.Framework.Water;
using SDG.Unturned;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Vector3 = UnityEngine.Vector3;

namespace Dummy.Threads
{
    public partial class DummyUserSimulationThread
    {
        private const float c_Swim = 3f;
        private const float c_Jump = 7f;
        private const float c_MinAngleSit = 60f;
        private const float c_MaxAngleSit = 120f;
        private const float c_MinAngleClimb = 45f;
        private const float c_MaxAngleClimb = 100f;
        private const float c_MinAngleSwim = 45f;
        private const float c_MaxAngleSwim = 135f;
        private const float c_MinAngleStand = 0f;
        private const float c_MaxAngleStand = 180f;
        private const float c_MinAngleCrouch = 20f;
        private const float c_MaxAngleCrouch = 160f;
        private const float c_MinAngleProne = 60f;
        private const float c_MaxAngleProne = 120f;

        private static readonly FieldInfo s_ServerSidePacketsField = typeof(PlayerInput).GetField("serversidePackets",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        private readonly ushort[] m_Flags;
        private readonly bool[] m_Keys;
        private readonly List<PlayerInputPacket> m_PlayerInputPackets;
        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;
        private Vector3 m_Velocity;

        private uint m_Count;
        private uint m_Buffer;
        private uint m_Consumed;
        private uint m_Simulation;
        private float m_Yaw;
        private float m_Pitch;
        private float m_TimeLerp;

        private Player Player => m_PlayerDummy.Player.Player;

        public bool Enabled { get; set; }

        /// <summary>
        /// Normalized move direction [-1 ; 1]. The <b>Y</b> direction should be always 0.
        /// </summary>
        public Vector3 Move { get; set; }

        public bool Jump // will be jumping until consume all stamina
        {
            get => m_Keys[0];
            set => m_Keys[0] = value;
        }

        public MouseState MouseState
        {
            get
            {
                var left = m_Keys[1] ? 1 : 0;
                var right = m_Keys[2] ? 2 : 0;

                return (MouseState)(left + right);
            }
            set
            {
                if (value is > MouseState.LeftRight or < MouseState.None)
                {
                    throw new ArgumentOutOfRangeException(nameof(MouseState));
                }

                var left = (value & MouseState.Left) != 0;
                var right = (value & MouseState.Right) != 0;

                m_Keys[1] = left;
                m_Keys[2] = right;
            }
        }

        public bool Crouch
        {
            get => m_Keys[3];
            set => m_Keys[3] = value;
        }

        public bool Prone
        {
            get => m_Keys[4];
            set => m_Keys[4] = value;
        }

        public bool Sprint // will be sprinting until consume all stamina
        {
            get => m_Keys[5];
            set => m_Keys[5] = value;
        }

        public bool LeanLeft
        {
            get => m_Keys[6];
            set => m_Keys[6] = value;
        }

        public bool LeanRight
        {
            get => m_Keys[7];
            set => m_Keys[7] = value;
        }

        public bool PluginKey1
        {
            get => m_Keys[10];
            set => m_Keys[10] = value;
        }

        public bool PluginKey2
        {
            get => m_Keys[11];
            set => m_Keys[11] = value;
        }

        public bool PluginKey3
        {
            get => m_Keys[12];
            set => m_Keys[12] = value;
        }

        public bool PluginKey4
        {
            get => m_Keys[13];
            set => m_Keys[13] = value;
        }

        public bool PluginKey5
        {
            get => m_Keys[14];
            set => m_Keys[14] = value;
        }

        public DummyUserSimulationThread(DummyUser playerDummy, ILogger logger)
        {
            m_PlayerDummy = playerDummy;
            m_Logger = logger;

            m_Count = 0;
            m_Buffer = 0;
            m_Consumed = 0;
            Move = Vector3.zero;
            m_PlayerInputPackets = new();

            var countKeys = 10 + ControlsSettings.NUM_PLUGIN_KEYS;
            m_Keys = new bool[countKeys];
            m_Flags = new ushort[countKeys];
            for (byte b = 0; b < countKeys; b++)
            {
                m_Flags[b] = (ushort)(1 << b);
            }
        }

        public void SetRotation(float yaw, float pitch, float time)
        {
            if (!yaw.IsFinite())
            {
                yaw = 0;
            }

            if (!pitch.IsFinite())
            {
                pitch = 0;
            }

            if (!time.IsFinite() || time <= 0)
            {
                time = 1f;
            }

            m_Yaw = yaw;
            m_Pitch = pitch;
            m_TimeLerp = time;

            ClampPitch();
            ClampYaw();
        }

        public async UniTask Start()
        {
            await UniTask.DelayFrame(5, PlayerLoopTiming.FixedUpdate);
            var queue = (Queue<PlayerInputPacket>)s_ServerSidePacketsField.GetValue(Player.input);

            while (Enabled)
            {
                await UniTask.WaitForFixedUpdate();
                if (!Enabled)
                {
                    // force exit
                    return;
                }

                // Do not simulate if dead
                if (Player.life.isDead)
                    continue;

                //var clampedVector = Move;
                //clampedVector.x = Mathf.Clamp((int)clampedVector.x, -1, 1);
                //clampedVector.y = 0;
                //clampedVector.z = Mathf.Clamp((int)clampedVector.z, -1, 1);
                //Move = clampedVector;

                if (m_Count % PlayerInput.SAMPLES == 0)
                {
                    for (var i = 0; i < m_Keys.Length; i++)
                    {
                        Player.input.keys[i] = m_Keys[i];
                    }

                    var movement = Player.movement;
                    var transform = Player.transform;
                    var normalizedMove = Move.normalized;
                    var speed = movement.speed;
                    var deltaTime = PlayerInput.RATE;
                    var stance = Player.stance.stance;
                    var controller = movement.controller;
                    var aim = Player.look.aim;

                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (stance)
                    {
                        case EPlayerStance.SITTING:
                            break;
                        case EPlayerStance.DRIVING:
                            SimulateVehicle();
                            break;
                        case EPlayerStance.CLIMB:
                            //s_FallProperty.SetValue(movement, c_Jump);
                            m_Velocity = new(0, Move.z * speed * 0.5f, 0);
                            controller.CheckedMove(m_Velocity * deltaTime);
                            break;
                        case EPlayerStance.SWIM:
                            if (Player.stance.isSubmerged || (Player.look.pitch > 110 && Move.z > 0.1f))
                            {
                                m_Velocity = aim.rotation * normalizedMove * speed * 1.5f;

                                if (Jump)
                                {
                                    m_Velocity.y = c_Swim * movement.pluginJumpMultiplier;
                                }

                                controller.CheckedMove(m_Velocity * deltaTime);
                                break;
                            }

                            WaterUtility.getUnderwaterInfo(transform.position, out var _, out var surfaceElevation);
                            m_Velocity = transform.rotation * Move.normalized * speed * 1.5f;
                            m_Velocity.y = (surfaceElevation - 1.275f - transform.position.y) / 8f;
                            controller.CheckedMove(m_Velocity * deltaTime);

                            break;
                        default:
                            var isMovementBlocked = false;
                            var shouldUpdateVelocity = false;
                            if (movement.isGrounded && movement.ground.normal.y > 0)
                            {
                                var slopeAngle = Vector3.Angle(Vector3.up, movement.ground.normal);
                                var maxWalkableSlope = 59f;
                                if (Level.info?.configData?.Max_Walkable_Slope > -0.5f)
                                {
                                    maxWalkableSlope = Level.info.configData.Max_Walkable_Slope;
                                }

                                if (slopeAngle > maxWalkableSlope)
                                {
                                    isMovementBlocked = true;
                                    var a = Vector3.Cross(Vector3.Cross(Vector3.up, movement.ground.normal), movement.ground.normal);
                                    m_Velocity += a * 16f * PlayerInput.RATE;
                                    shouldUpdateVelocity = true;
                                }
                            }

                            if (!isMovementBlocked)
                            {
                                var moveVector = movement.transform.rotation * normalizedMove * speed;

                                if (movement.isGrounded)
                                {
                                    moveVector = Vector3.Cross(Vector3.Cross(Vector3.up, moveVector), movement.ground.normal);
                                    moveVector.y = Mathf.Min(moveVector.y, 0f);

                                    // it should also change direction on where is a dummy stand on (exmaple: ice)
                                    // not possible to use because is internal and WIP (unstable to use it)

                                    m_Velocity = moveVector;
                                }
                                else
                                {
                                    m_Velocity.y += Physics.gravity.y * ((movement.fall <= 0f) ? movement.totalGravityMultiplier : 1f) * deltaTime * 3f;
                                    var maxFall = (movement.totalGravityMultiplier < 0.99f) ? (Physics.gravity.y * 2f * movement.totalGravityMultiplier) : -100f;
                                    m_Velocity.y = Mathf.Max(maxFall, m_Velocity.y);

                                    var horizontalMagnitude = moveVector.GetHorizontalMagnitude();
                                    var horizontal = m_Velocity.GetHorizontal();
                                    var horizontalMagnitude2 = m_Velocity.GetHorizontalMagnitude();
                                    float maxMagnitude;
                                    if (horizontalMagnitude2 > horizontalMagnitude)
                                    {
                                        var num5 = 2f * Provider.modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier;
                                        maxMagnitude = Mathf.Max(horizontalMagnitude, horizontalMagnitude2 - (num5 * deltaTime));
                                    }
                                    else
                                    {
                                        maxMagnitude = horizontalMagnitude;
                                    }

                                    var a3 = moveVector * (4f * Provider.modeConfigData.Gameplay.AirStrafing_Acceleration_Multiplier);
                                    var vector2 = horizontal + (a3 * deltaTime);
                                    vector2 = vector2.ClampHorizontalMagnitude(maxMagnitude);
                                    m_Velocity.x = vector2.x;
                                    m_Velocity.z = vector2.z;
                                    shouldUpdateVelocity = true;
                                }
                            }

                            var jumpMastery = Player.skills.mastery(0, 6);
                            if (Jump && movement.isGrounded && !Player.life.isBroken &&
                                Player.life.stamina >= 10f * (1f - (jumpMastery * 0.5f)) &&
                                stance is EPlayerStance.STAND or EPlayerStance.SPRINT)
                            {
                                m_Velocity.y = c_Jump * (1f + (jumpMastery * 0.25f)) * movement.pluginJumpMultiplier;
                                Player.life.askTire((byte)(10f * (1f - (jumpMastery * 0.5f))));
                            }

                            m_Velocity += movement.pendingLaunchVelocity;
                            movement.pendingLaunchVelocity = Vector3.zero;

                            var previousPosition = movement.transform.position;
                            movement.controller.CheckedMove(m_Velocity * deltaTime);

                            if (shouldUpdateVelocity)
                            {
                                m_Velocity = (movement.transform.position - previousPosition) / deltaTime;
                            }
                            break;
                    }

                    PlayerInputPacket packet;
                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        m_PlayerInputPackets.Add(packet = new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        m_PlayerInputPackets.Add(packet = new WalkingPlayerInputPacket());
                    }

                    // recov can be ignored
                    packet.clientSimulationFrameNumber = m_Simulation;

                    m_Simulation++;
                    m_Buffer += PlayerInput.SAMPLES;
                }

                if (m_Consumed < m_Buffer)
                {
                    m_Consumed++;
                }

                if (m_Consumed == m_Buffer && m_PlayerInputPackets.Count > 0)
                {
                    ushort compressedKeys = 0;
                    for (var b = 0; b < m_Keys.Length; b++)
                    {
                        if (m_Keys[b])
                        {
                            compressedKeys |= m_Flags[b];
                        }
                    }

                    var playerInputPacket2 = m_PlayerInputPackets[^1];
                    playerInputPacket2.keys = compressedKeys;

                    switch (playerInputPacket2)
                    {
                        case DrivingPlayerInputPacket drivingPlayerInputPacket:
                            var vehicle = Player.movement.getVehicle();

                            if (vehicle != null)
                            {
                                var transform = vehicle.transform;
                                drivingPlayerInputPacket.position = vehicle.asset.engine == EEngine.TRAIN
                                    ? new(vehicle.roadPosition, 0f, 0f)
                                    : vehicle.transform.position;

                                drivingPlayerInputPacket.rotation = transform.rotation;

                                drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.physicsSpeed =
                                    (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.turn = (byte)(Move.x + 1);
                            }

                            break;
                        case WalkingPlayerInputPacket walkingPlayerInputPacket:
                            var horizontal = (byte)(Move.x + 1);
                            var vertical = (byte)(Move.y + 1);

                            walkingPlayerInputPacket.analog = (byte)((horizontal << 4) | vertical);
                            walkingPlayerInputPacket.clientPosition = Player.transform.position;
                            walkingPlayerInputPacket.yaw = Mathf.Lerp(Player.look.yaw, m_Yaw, m_TimeLerp);
                            walkingPlayerInputPacket.pitch = Mathf.Lerp(Player.look.pitch, m_Pitch, m_TimeLerp);
                            break;
                    }

                    foreach (var playerInputPacket in m_PlayerInputPackets)
                    {
                        queue.Enqueue(playerInputPacket);
                    }

                    m_PlayerInputPackets.Clear();
                }

                m_Count++;
            }
        }

        private void ClampPitch()
        {
            var vehicleSeat = Player.movement.getVehicleSeat();
            var min = 0f;
            var max = 180f;

            if (vehicleSeat != null)
            {
                if (vehicleSeat.turret != null)
                {
                    min = vehicleSeat.turret.pitchMin;
                    max = vehicleSeat.turret.pitchMax;
                }
                else
                {
                    min = c_MinAngleSit;
                    max = c_MaxAngleSit;
                }
            }
            else
            {
                switch (Player.stance.stance)
                {
                    case EPlayerStance.STAND or EPlayerStance.SPRINT:
                        min = c_MinAngleStand;
                        max = c_MaxAngleStand;
                        break;
                    case EPlayerStance.CLIMB:
                        min = c_MinAngleClimb;
                        max = c_MaxAngleClimb;
                        break;
                    case EPlayerStance.SWIM:
                        min = c_MinAngleSwim;
                        max = c_MaxAngleSwim;
                        break;
                    case EPlayerStance.CROUCH:
                        min = c_MinAngleCrouch;
                        max = c_MaxAngleCrouch;
                        break;
                    case EPlayerStance.PRONE:
                        min = c_MinAngleProne;
                        max = c_MaxAngleProne;
                        break;
                }
            }

            m_Pitch = Mathf.Clamp(m_Pitch, min, max);
        }

        private void ClampYaw()
        {
            m_Yaw %= 360f;
            var vehicleSeat = Player.movement.getVehicleSeat();
            if (vehicleSeat == null)
            {
                return;
            }

            var min = -90f;
            var max = 90f;
            if (vehicleSeat.turret != null)
            {
                min = vehicleSeat.turret.yawMin;
                max = vehicleSeat.turret.yawMax;
            }
            else if (Player.stance.stance == EPlayerStance.DRIVING)
            {
                min = -160f;
                max = 160f;
            }

            m_Yaw = Mathf.Clamp(m_Yaw, min, max);
        }
    }
}
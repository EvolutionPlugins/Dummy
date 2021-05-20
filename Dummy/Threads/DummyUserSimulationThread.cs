using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Framework.Water;
using SDG.Unturned;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Dummy.Threads
{
    public class DummyUserSimulationThread
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

        private static readonly PropertyInfo s_FallProperty =
            typeof(PlayerMovement).GetProperty(nameof(PlayerMovement.fall),
                BindingFlags.Instance | BindingFlags.Public)!;

        private readonly ushort[] m_Flags;
        private readonly List<PlayerInputPacket> m_PlayerInputPackets;
        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;

        private uint m_Count;
        private uint m_Buffer;
        private uint m_Consumed;
        private Vector3 m_Direction;
        private float m_Slope;
        private float m_Fall2;
        private float m_Yaw;
        private float m_Pitch;
        private float m_TimeLerp;

        private Player Player => m_PlayerDummy.Player.Player;

        public bool Enabled { get; set; }
        public Vector2 Move { get; set; } // set only X and Y props at int range [-1;1]
        public bool Jump { get; set; } // will be jumping until consume all stamina
        public bool Sprint { get; set; } // will be sprinting until consume all stamina

        public DummyUserSimulationThread(DummyUser playerDummy, ILogger logger)
        {
            m_PlayerDummy = playerDummy;
            m_Logger = logger;

            m_Count = 0;
            m_Buffer = 0;
            m_Consumed = 0;
            Move = Vector2.zero;
            Jump = false;
            m_PlayerInputPackets = new();
            m_Flags = new ushort[9 + ControlsSettings.NUM_PLUGIN_KEYS];
            for (byte b = 0; b < 9 + ControlsSettings.NUM_PLUGIN_KEYS; b++)
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

                var clampedVector = Move;
                clampedVector.x = Mathf.Clamp((int)clampedVector.x, -1, 1);
                clampedVector.y = Mathf.Clamp((int)clampedVector.y, -1, 1);
                Move = clampedVector;

                if (m_Count % PlayerInput.SAMPLES == 0)
                {
                    Player.input.keys[0] = Jump;
                    Player.input.keys[1] = Player.equipment.primary;
                    Player.input.keys[2] = Player.equipment.secondary;
                    Player.input.keys[3] = Player.stance.crouch;
                    Player.input.keys[4] = Player.stance.prone;
                    Player.input.keys[5] = Sprint;
                    Player.input.keys[6] = Player.animator.leanLeft;
                    Player.input.keys[7] = Player.animator.leanRight;
                    Player.input.keys[8] = false;

                    for (var i = 0; i < ControlsSettings.NUM_PLUGIN_KEYS; i++)
                    {
                        var num = Player.input.keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + i;
                        Player.input.keys[num] = false; // todo
                    }

                    var movement = Player.movement;
                    var material = GetMaterialAtPlayer();
                    var rotation = Player.transform.rotation;
                    var normalizedMove = Move.normalized;
                    var speed = movement.speed;
                    var delta = PlayerInput.RATE;
                    var stance = Player.stance.stance;
                    var controller = movement.controller;
                    var landscapeHoleVolume = movement.landscapeHoleVolume;

                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (stance)
                    {
                        case EPlayerStance.CLIMB:
                            s_FallProperty.SetValue(movement, c_Jump);
                            m_Direction = normalizedMove * speed / 2;
                            controller.CheckedMove(Vector3.up * m_Direction.z * delta, landscapeHoleVolume != null);
                            break;
                        case EPlayerStance.SWIM:
                        {
                            m_Direction = normalizedMove * speed * 1.5f;
                            if (Player.stance.isSubmerged || Player.look.pitch > 110 && Move.y > 0.1f)
                            {
                                var fall = Jump
                                    ? c_Swim * movement.pluginJumpMultiplier
                                    : movement.fall + Physics.gravity.y * delta / 7f;
                                if (fall < Physics.gravity.y / 7f)
                                {
                                    fall = Physics.gravity.y / 7f;
                                }

                                s_FallProperty.SetValue(movement, fall);
                                controller.CheckedMove(
                                    Player.look.aim.rotation * m_Direction * delta + Vector3.up * fall * delta,
                                    landscapeHoleVolume != null);
                            }
                            else
                            {
                                controller.CheckedMove(
                                    rotation * m_Direction * delta + Vector3.up * movement.fall * delta,
                                    landscapeHoleVolume != null);
                            }

                            break;
                        }
                        default:
                        {
                            var fall = movement.fall + Physics.gravity.y *
                                (movement.fall <= 0f ? movement.totalGravityMultiplier : 1f) * delta * 3f;
                            if (fall < Physics.gravity.y * 2f * movement.totalGravityMultiplier)
                            {
                                fall = Physics.gravity.y * 2f * movement.totalGravityMultiplier;
                            }

                            var jumpMastery = Player.skills.mastery(0, 6);

                            if (Jump && movement.isGrounded && !Player.life.isBroken &&
                                Player.life.stamina >= 10f * (1f - jumpMastery * 0.5f) &&
                                stance is EPlayerStance.STAND or EPlayerStance.SPRINT)
                            {
                                fall = c_Jump * (1f + jumpMastery * movement.pluginJumpMultiplier);
                                Player.life.askTire((byte)(10f * (1f - jumpMastery * 0.5f)));
                            }

                            s_FallProperty.SetValue(movement, fall);

                            if (movement.isGrounded && movement.ground.transform != null &&
                                movement.ground.normal.y > 0)
                            {
                                m_Slope = Mathf.Lerp(m_Slope, Mathf.Max(movement.ground.normal.y, 0.01f), delta);
                            }
                            else
                            {
                                m_Slope = Mathf.Lerp(m_Slope, 1f, delta);
                            }

                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                            switch (material)
                            {
                                case EPhysicsMaterial.ICE_STATIC:
                                    m_Direction = Vector3.Lerp(m_Direction,
                                        rotation * normalizedMove * speed * m_Slope * delta,
                                        delta);
                                    break;
                                case EPhysicsMaterial.METAL_SLIP:
                                {
                                    var num2 = m_Slope < 0.75f ? 0f : Mathf.Lerp(0f, 1f, (m_Slope - 0.75f) * 4f);

                                    m_Direction = Vector3.Lerp(m_Direction,
                                        rotation * normalizedMove * speed * m_Slope * delta * 2f,
                                        movement.isMoving ? 2f * delta : 0.5f * num2 * delta);
                                    break;
                                }
                                default:
                                    m_Direction = rotation * normalizedMove * speed * m_Slope * delta;
                                    break;
                            }

                            var vector = m_Direction;
                            if (movement.isGrounded && movement.ground.normal.y > 0)
                            {
                                var angleSlope = Vector3.Angle(Vector3.up, movement.ground.normal);
                                var maxAngleSlope = 59f;
                                if (Level.info?.configData?.Max_Walkable_Slope > -0.5f)
                                {
                                    maxAngleSlope = Level.info.configData.Max_Walkable_Slope;
                                }

                                if (angleSlope > maxAngleSlope)
                                {
                                    m_Fall2 += 16f * delta;
                                    if (m_Fall2 > 128f)
                                    {
                                        m_Fall2 = 128f;
                                    }

                                    var a = Vector3.Cross(Vector3.Cross(Vector3.up, movement.ground.normal),
                                        movement.ground.normal);
                                    vector += a * m_Fall2 * delta;
                                }
                                else
                                {
                                    m_Fall2 = 0;
                                }
                            }

                            vector += Vector3.up * fall * delta;
                            controller.CheckedMove(vector, movement.landscapeHoleVolume != null);
                            break;
                        }
                    }

                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        m_PlayerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        m_PlayerInputPackets.Add(new WalkingPlayerInputPacket());
                    }

                    m_Buffer += PlayerInput.SAMPLES;
                }

                if (m_Consumed < m_Buffer)
                {
                    m_Consumed++;
                }

                if (m_Consumed == m_Buffer && m_PlayerInputPackets.Count > 0)
                {
                    ushort compressedKeys = 0;
                    for (byte b = 0; b < Player.input.keys.Length; b++)
                    {
                        if (Player.input.keys[b])
                        {
                            compressedKeys |= m_Flags[b];
                        }
                    }

                    var playerInputPacket2 = m_PlayerInputPackets.Last();
                    playerInputPacket2.keys = compressedKeys;

                    switch (playerInputPacket2)
                    {
                        case DrivingPlayerInputPacket drivingPlayerInputPacket:
                        {
                            var vehicle = Player.movement.getVehicle();

                            if (vehicle != null)
                            {
                                var transform = vehicle.transform;
                                drivingPlayerInputPacket.position = vehicle.asset.engine == EEngine.TRAIN
                                    ? new(vehicle.roadPosition, 0f, 0f)
                                    : transform.position;

                                drivingPlayerInputPacket.rotation = transform.rotation;

                                drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.physicsSpeed =
                                    (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.turn = (byte)(Move.x + 1);
                            }

                            break;
                        }
                        case WalkingPlayerInputPacket walkingPlayerInputPacket:
                            var horizontal = (byte)Move.x + 1;
                            var vertical = (byte)Move.y + 1;
                            
                            Console.WriteLine(Mathf.Lerp(Player.look.yaw, m_Yaw, m_TimeLerp));
                            Console.WriteLine(Mathf.Lerp(Player.look.pitch, m_Pitch, m_TimeLerp));

                            walkingPlayerInputPacket!.analog =
                                (byte)(horizontal << 4 | vertical);
                            walkingPlayerInputPacket.position = Player.transform.position;
                            walkingPlayerInputPacket.yaw = Mathf.Lerp(Player.look.yaw, m_Yaw, m_TimeLerp);
                            walkingPlayerInputPacket.pitch = Mathf.Lerp(Player.look.pitch, m_Pitch, m_TimeLerp);
                            break;
                    }

                    foreach (var playerInputPacket3 in m_PlayerInputPackets)
                    {
                        queue.Enqueue(playerInputPacket3);
                    }

                    m_PlayerInputPackets.Clear();
                }

                m_Count++;
            }
        }

        private EPhysicsMaterial GetMaterialAtPlayer()
        {
            var movement = Player.movement;

            if (Player.stance.stance == EPlayerStance.CLIMB)
            {
                return EPhysicsMaterial.TILE_STATIC;
            }

            if (Player.stance.stance == EPlayerStance.SWIM || WaterUtility.isPointUnderwater(Player.transform.position))
            {
                return EPhysicsMaterial.WATER_STATIC;
            }

            if (movement.ground.transform == null)
            {
                return EPhysicsMaterial.NONE;
            }

            return movement.ground.transform.CompareTag("Ground")
                ? PhysicsTool.checkMaterial(Player.transform.position)
                : PhysicsTool.checkMaterial(movement.ground.collider);
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
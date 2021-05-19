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
        private static readonly FieldInfo s_ServerSidePacketsField = typeof(PlayerInput).GetField("serversidePackets",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly PropertyInfo s_FallProperty =
            typeof(PlayerMovement).GetProperty(nameof(PlayerMovement.fall),
                BindingFlags.Instance | BindingFlags.Public)!;

        private static readonly float SWIM = 3f;
        private static readonly float JUMP = 7f;

        public bool Enabled { get; set; }
        public uint Simulation { get; private set; }
        public Vector3 Move { get; set; } // set only X and Y props
        public bool Jump { get; set; } // will be jumping until consume all stamina
        public bool Sprint { get; set; } // will be sprinting until consume all stamina

        private uint Count { get; set; }
        private uint Buffer { get; set; }
        private uint Consumed { get; set; }
        private ushort[] Flags { get; }
        private Vector3 Direction { get; set; }
        private float Slope { get; set; }
        private float Fall2 { get; set; }
        private List<PlayerInputPacket> PlayerInputPackets { get; }
        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;

        private Player Player => m_PlayerDummy.Player.Player;

        public DummyUserSimulationThread(DummyUser playerDummy, ILogger logger)
        {
            m_PlayerDummy = playerDummy;
            m_Logger = logger;

            Count = 0;
            Buffer = 0;
            Simulation = 0;
            Consumed = 0;
            Move = Vector3.zero;
            Jump = false;
            PlayerInputPackets = new();
            Flags = new ushort[9 + ControlsSettings.NUM_PLUGIN_KEYS];
            for (byte b = 0; b < 9 + ControlsSettings.NUM_PLUGIN_KEYS; b++)
            {
                Flags[b] = (ushort)(1 << b);
            }
        }

        public async UniTask Start()
        {
            await UniTask.DelayFrame(5, PlayerLoopTiming.FixedUpdate);
            var queue = (Queue<PlayerInputPacket>)s_ServerSidePacketsField.GetValue(Player.input);

            while (Enabled)
            {
                await UniTask.WaitForFixedUpdate();

                if (Count % PlayerInput.SAMPLES == 0)
                {
                    Player.input.keys[0] = Player.movement.jump || Jump;
                    Player.input.keys[1] = Player.equipment.primary;
                    Player.input.keys[2] = Player.equipment.secondary;
                    Player.input.keys[3] = Player.stance.crouch;
                    Player.input.keys[4] = Player.stance.prone;
                    Player.input.keys[5] = Player.stance.sprint || Sprint;
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
                            s_FallProperty.SetValue(movement, JUMP);
                            Direction = normalizedMove * speed / 2;
                            controller.CheckedMove(Vector3.up * Direction.z * delta, landscapeHoleVolume != null);
                            break;
                        case EPlayerStance.SWIM:
                        {
                            Direction = normalizedMove * speed * 1.5f;
                            if (Player.stance.isSubmerged || Player.look.pitch > 110 && Move.z > 0.1f)
                            {
                                var fall = Jump
                                    ? SWIM * movement.pluginJumpMultiplier
                                    : movement.fall + Physics.gravity.y * delta / 7f;
                                if (fall < Physics.gravity.y / 7f)
                                {
                                    fall = Physics.gravity.y / 7f;
                                }

                                s_FallProperty.SetValue(movement, fall);
                                controller.CheckedMove(
                                    Player.look.aim.rotation * Direction * delta + Vector3.up * fall * delta,
                                    landscapeHoleVolume != null);
                            }
                            else
                            {
                                controller.CheckedMove(
                                    rotation * Direction * delta + Vector3.up * movement.fall * delta,
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
                                fall = JUMP * (1f + jumpMastery * movement.pluginJumpMultiplier);
                                Player.life.askTire((byte)(10f * (1f - jumpMastery * 0.5f)));
                            }
                            
                            s_FallProperty.SetValue(movement, fall);

                            if (movement.isGrounded && movement.ground.transform != null &&
                                movement.ground.normal.y > 0)
                            {
                                Slope = Mathf.Lerp(Slope, Mathf.Max(movement.ground.normal.y, 0.01f), delta);
                            }
                            else
                            {
                                Slope = Mathf.Lerp(Slope, 1f, delta);
                            }

                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                            switch (material)
                            {
                                case EPhysicsMaterial.ICE_STATIC:
                                    Direction = Vector3.Lerp(Direction,
                                        rotation * normalizedMove * speed * Slope * delta,
                                        delta);
                                    break;
                                case EPhysicsMaterial.METAL_SLIP:
                                {
                                    var num2 = Slope < 0.75f ? 0f : Mathf.Lerp(0f, 1f, (Slope - 0.75f) * 4f);

                                    Direction = Vector3.Lerp(Direction,
                                        rotation * normalizedMove * speed * Slope * delta * 2f,
                                        movement.isMoving ? 2f * delta : 0.5f * num2 * delta);
                                    break;
                                }
                                default:
                                    Direction = rotation * normalizedMove * speed * Slope * delta;
                                    break;
                            }

                            var vector = Direction;
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
                                    Fall2 += 16f * delta;
                                    if (Fall2 > 128f)
                                    {
                                        Fall2 = 128f;
                                    }

                                    var a = Vector3.Cross(Vector3.Cross(Vector3.up, movement.ground.normal),
                                        movement.ground.normal);
                                    vector += a * Fall2 * delta;
                                }
                                else
                                {
                                    Fall2 = 0;
                                }
                            }

                            vector += Vector3.up * fall * delta;
                            controller.CheckedMove(vector, movement.landscapeHoleVolume != null);
                            break;
                        }
                    }

                    if (Player.stance.stance == EPlayerStance.DRIVING)
                    {
                        PlayerInputPackets.Add(new DrivingPlayerInputPacket());
                    }
                    else
                    {
                        PlayerInputPackets.Add(new WalkingPlayerInputPacket());
                    }

                    Buffer += PlayerInput.SAMPLES;
                    Simulation++;
                }

                if (Consumed < Buffer)
                {
                    Consumed++;
                }

                if (Consumed == Buffer && PlayerInputPackets.Count > 0)
                {
                    ushort compressedKeys = 0;
                    for (byte b = 0; b < Player.input.keys.Length; b++)
                    {
                        if (Player.input.keys[b])
                        {
                            compressedKeys |= Flags[b];
                        }
                    }

                    var playerInputPacket2 = PlayerInputPackets.Last();
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
                                drivingPlayerInputPacket.turn = (byte)(vehicle.turn + 1);
                            }

                            break;
                        }
                        case WalkingPlayerInputPacket walkingPlayerInputPacket:
                            walkingPlayerInputPacket!.analog =
                                (byte)(Player.movement.horizontal << 4 | Player.movement.vertical);
                            walkingPlayerInputPacket.position = Player.transform.position;
                            walkingPlayerInputPacket.yaw = Player.look.yaw;
                            walkingPlayerInputPacket.pitch = Player.look.pitch;
                            break;
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
    }
}
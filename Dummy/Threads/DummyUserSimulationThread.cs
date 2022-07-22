using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly bool[] m_Keys;
        private readonly List<PlayerInputPacket> m_PlayerInputPackets;
        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;
        private Vector3 m_Velocity;

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

        public Vector3 Move { get; set; } // set only X and Z props at int range [-1;1]

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
            get => m_Keys[9];
            set => m_Keys[9] = value;
        }

        public bool PluginKey2
        {
            get => m_Keys[10];
            set => m_Keys[10] = value;
        }

        public bool PluginKey3
        {
            get => m_Keys[11];
            set => m_Keys[11] = value;
        }

        public bool PluginKey4
        {
            get => m_Keys[12];
            set => m_Keys[12] = value;
        }

        public bool PluginKey5
        {
            get => m_Keys[13];
            set => m_Keys[13] = value;
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
            m_Keys = new bool[9 + ControlsSettings.NUM_PLUGIN_KEYS];

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
                await UniTask.SwitchToMainThread();
                await UniTask.WaitForFixedUpdate();

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
                        case EPlayerStance.SITTING:
                            break;
                        case EPlayerStance.DRIVING:
                            await SimulateVehicle();
                            break;
                        case EPlayerStance.CLIMB:
                            s_FallProperty.SetValue(movement, c_Jump);
                            m_Direction = normalizedMove * speed / 2;
                            controller.CheckedMove(Vector3.up * m_Direction.z * delta, landscapeHoleVolume != null);
                            break;
                        case EPlayerStance.SWIM:
                        {
                            m_Direction = normalizedMove * speed * 1.5f;
                            if (Player.stance.isSubmerged || (Player.look.pitch > 110 && Move.z > 0.1f))
                            {
                                var fall = Jump
                                    ? c_Swim * movement.pluginJumpMultiplier
                                    : movement.fall + (Physics.gravity.y * delta / 7f);
                                if (fall < Physics.gravity.y / 7f)
                                {
                                    fall = Physics.gravity.y / 7f;
                                }

                                s_FallProperty.SetValue(movement, fall);
                                controller.CheckedMove(
                                    (Player.look.aim.rotation * m_Direction * delta) + (Vector3.up * fall * delta),
                                    landscapeHoleVolume != null);
                            }
                            else
                            {
                                controller.CheckedMove(
                                    (rotation * m_Direction * delta) + (Vector3.up * movement.fall * delta),
                                    landscapeHoleVolume != null);
                            }

                            break;
                        }
                        default:
                        {
                            var fall = movement.fall + (Physics.gravity.y *
                                (movement.fall <= 0f ? movement.totalGravityMultiplier : 1f) * delta * 3f);
                            if (fall < Physics.gravity.y * 2f * movement.totalGravityMultiplier)
                            {
                                fall = Physics.gravity.y * 2f * movement.totalGravityMultiplier;
                            }

                            var jumpMastery = Player.skills.mastery(0, 6);

                            if (Jump && movement.isGrounded && !Player.life.isBroken &&
                                Player.life.stamina >= 10f * (1f - (jumpMastery * 0.5f)) &&
                                stance is EPlayerStance.STAND or EPlayerStance.SPRINT)
                            {
                                fall = c_Jump * (1f + (jumpMastery * movement.pluginJumpMultiplier));
                                Player.life.askTire((byte)(10f * (1f - (jumpMastery * 0.5f))));
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
                    for (var b = 0; b < m_Keys.Length; b++)
                    {
                        if (m_Keys[b])
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
                                    : vehicle.transform.position;

                                drivingPlayerInputPacket.rotation = transform.rotation;

                                drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.physicsSpeed =
                                    (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                                drivingPlayerInputPacket.turn = (byte)(Move.x + 1);
                            }

                            break;
                        }
                        case WalkingPlayerInputPacket walkingPlayerInputPacket:
                            var horizontal = (byte)(Move.x + 1);
                            var vertical = (byte)(Move.y + 1);

                            walkingPlayerInputPacket.analog =
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

        private readonly static PropertyInfo s_IsBoostingProperty = typeof(InteractableVehicle).GetProperty(nameof(InteractableVehicle.isBoosting),
            BindingFlags.Public | BindingFlags.Instance);
        private readonly static FieldInfo s_SpeedField = typeof(InteractableVehicle).GetField("_speed",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_PhysicsSpeedField = typeof(InteractableVehicle).GetField("_physicsSpeed",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_FactorField = typeof(InteractableVehicle).GetField("_factor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_BuoyancyField = typeof(InteractableVehicle).GetField("buoyancy",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_SpeedTractionField = typeof(InteractableVehicle).GetField("speedTraction",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_LastUpdatedPosField = typeof(InteractableVehicle).GetField("lastUpdatedPos",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo s_IsPhysicalField = typeof(InteractableVehicle).GetField("isPhysical",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private float m_Factor;
        private Rigidbody? m_Rigidbody;
        private Transform? m_Buoyancy;
        private float m_AltSpeedInput;
        private float m_AltSpeedOutput;
        private float m_SpeedTraction;

        private async UniTask SimulateVehicle()
        {
            // im funkin dead to make that
            // also, my VS crashed about 3 times when writing this code

            await UniTask.SwitchToMainThread();

            var vehicle = Player.movement.getVehicle();
            if (vehicle == null)
            {
                m_Factor = 0f;
                m_Rigidbody = null;
                m_Buoyancy = null;
                m_AltSpeedInput = 0;
                m_AltSpeedOutput = 0;
                m_SpeedTraction = 0;
                return;
            }

            var asset = vehicle.asset;
            m_Factor = (float)s_FactorField.GetValue(vehicle);
            m_SpeedTraction = (float)s_SpeedTractionField.GetValue(vehicle);

            if (!m_Buoyancy)
            {
                m_Buoyancy = (Transform)s_BuoyancyField.GetValue(vehicle);
            }

            if (!m_Rigidbody)
            {
                m_Rigidbody = vehicle.GetComponent<Rigidbody>();
            }

            // update physics
            m_Rigidbody!.useGravity = asset.engine != EEngine.TRAIN;
            m_Rigidbody.isKinematic = asset.engine == EEngine.TRAIN;
            foreach (var tire in vehicle.tires)
            {
                tire.isPhysical = true;
            }

            var moveY = Move.z;
            var speed = 1f;

            if (asset.useStaminaBoost)
            {
                if (Sprint && vehicle.passengers[0]?.player?.player.life.stamina > 0)
                {
                    s_IsBoostingProperty.SetValue(vehicle, true);
                }
                else
                {
                    s_IsBoostingProperty.SetValue(vehicle, false);
                    moveY *= asset.staminaBoost;
                    speed *= asset.staminaBoost;
                }
            }
            else
            {
                s_IsBoostingProperty.SetValue(vehicle, false);
            }

            Console.WriteLine("sim");
            Console.WriteLine(s_IsPhysicalField.GetValue(vehicle));
            s_SpeedField.SetValue(vehicle, 150f);
            s_IsPhysicalField.SetValue(vehicle, true);

            if ((vehicle.usesFuel && vehicle.fuel == 0) || vehicle.isUnderwater || vehicle.isDead || !vehicle.isEngineOn)
            {
                moveY = 0;
                speed = 1f;
            }

            m_Factor = Mathf.InverseLerp(0f, (vehicle.speed < 0) ? asset.speedMin : asset.speedMax, vehicle.speed);
            var tireOnGround = false;

            if (vehicle.tires != null)
            {
                foreach (var tire in vehicle.tires)
                {
                    tire.simulate(Move.x, moveY, Jump /* inputBrake */, PlayerInput.RATE);
                    tire.update(Time.deltaTime);
                    tireOnGround |= tire.isGrounded;
                }
            }

            Console.WriteLine(m_Factor);
            Console.WriteLine(moveY);
            Console.WriteLine(speed);
            Console.WriteLine(tireOnGround);
            Console.WriteLine(asset.engine);

            switch (asset.engine)
            {
                case EEngine.CAR:
                    if (tireOnGround)
                    {
                        Console.WriteLine(m_Rigidbody.velocity);
                        m_Rigidbody.AddForce(-vehicle.transform.up * m_Factor * 40f);
                        m_Rigidbody.AddForce(vehicle.transform.forward * 20f);
                        Console.WriteLine(m_Rigidbody.velocity);
                    }

                    if (m_Buoyancy != null)
                    {
                        var lerpSteerCar = Mathf.Lerp(asset.airSteerMax, asset.airSteerMin, m_Factor);
                        var isUnderWater = WaterUtility.isPointUnderwater(vehicle.transform.position - Vector3.up);
                        m_SpeedTraction = Mathf.Lerp(m_SpeedTraction, isUnderWater ? 0f : 1f, 4f * Time.deltaTime);

                        if (!MathfEx.IsNearlyZero(m_SpeedTraction))
                        {
                            m_AltSpeedInput = moveY switch
                            {
                                > 0 => Mathf.Lerp(m_AltSpeedInput, asset.speedMax, PlayerInput.RATE / 4f),
                                < 0 => Mathf.Lerp(m_AltSpeedInput, asset.speedMin, PlayerInput.RATE / 4f),
                                _ => Mathf.Lerp(m_AltSpeedInput, 0, PlayerInput.RATE / 8f),
                            };
                            m_AltSpeedOutput = m_AltSpeedInput * m_SpeedTraction;

                            Console.WriteLine(m_AltSpeedInput);
                            Console.WriteLine(m_AltSpeedOutput);

                            var forward = vehicle.transform.forward;
                            forward.y = 0;

                            m_Rigidbody!.AddForce(forward.normalized * m_AltSpeedOutput * 2f * m_SpeedTraction);
                            m_Rigidbody.AddRelativeTorque(Move.z * -2.5f * m_SpeedTraction, Move.x * lerpSteerCar / 8f * m_SpeedTraction,
                                Move.x * -2.5f * m_SpeedTraction);
                        }
                    }
                    break;
                case EEngine.PLANE:
                    var lerpSteer = Mathf.Lerp(asset.airSteerMax, asset.airSteerMin, m_Factor);
                    m_AltSpeedInput = lerpSteer switch
                    {
                        > 0 => Mathf.Lerp(m_AltSpeedInput, asset.speedMax * speed, PlayerInput.RATE),
                        < 0 => Mathf.Lerp(m_AltSpeedInput, 0, PlayerInput.RATE / 8f),
                        _ => Mathf.Lerp(m_AltSpeedInput, 0, PlayerInput.RATE / 16f),
                    };
                    m_AltSpeedOutput = m_AltSpeedInput;

                    m_Rigidbody.AddForce(vehicle.transform.forward * m_AltSpeedOutput * 2f);
                    m_Rigidbody.AddForce(Mathf.Lerp(0f, 1f, vehicle.transform.InverseTransformDirection(m_Rigidbody.velocity).z / asset.speedMax)
                        * asset.lift * -Physics.gravity);

                    if (vehicle.tires == null || vehicle.tires.Length == 0 || (!vehicle.tires[0].isGrounded && !vehicle.tires[1].isGrounded))
                    {
                        m_Rigidbody.AddRelativeTorque(Mathf.Clamp(Move.z, -asset.airTurnResponsiveness, asset.airTurnResponsiveness)
                            * lerpSteer, Move.x * asset.airTurnResponsiveness * lerpSteer / 4f,
                                Mathf.Clamp(Move.x, -asset.airTurnResponsiveness, asset.airTurnResponsiveness) * -lerpSteer / 2f);
                    }

                    if (vehicle.tires == null || (vehicle.tires.Length == 0 && moveY < 0))
                    {
                        m_Rigidbody.AddForce(vehicle.transform.forward * asset.speedMin * 4f);
                    }

                    break;
                case EEngine.HELICOPTER:
                    break;
                case EEngine.BLIMP:
                    break;
                case EEngine.BOAT:
                    break;
                case EEngine.TRAIN:
                    break;
            }

            var _speed = asset.engine switch
            {
                EEngine.CAR => vehicle.transform.InverseTransformDirection(m_Rigidbody!.velocity).z,
                EEngine.TRAIN => m_AltSpeedOutput,
                _ => m_AltSpeedOutput
            };

            var _physicsSpeed = asset.engine switch
            {
                EEngine.CAR => _speed,
                EEngine.TRAIN => m_AltSpeedOutput,
                _ => vehicle.transform.InverseTransformDirection(m_Rigidbody!.velocity).z
            };

            Console.WriteLine("s:" + _speed);
            Console.WriteLine("s:" + _physicsSpeed);

            s_SpeedField.SetValue(vehicle, _speed);
            s_PhysicsSpeedField.SetValue(vehicle, _physicsSpeed);
            s_FactorField.SetValue(vehicle, m_Factor);
            s_SpeedTractionField.SetValue(vehicle, m_SpeedTraction);
            s_LastUpdatedPosField.SetValue(vehicle, vehicle.transform.position);
        }
    }
}
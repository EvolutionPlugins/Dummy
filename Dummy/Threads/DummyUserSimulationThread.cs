using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction;
using Dummy.Patches;
using Dummy.Users;
using SDG.Framework.Water;
using SDG.Unturned;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Dummy.Threads
{
    public partial class DummyUserSimulationThread : IAsyncDisposable
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
        private readonly DummyUser m_PlayerDummy;
        private readonly ILogger m_Logger;

        private PlayerInputPacket m_Packet;
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
            m_Packet = new();

            var countKeys = 10 + ControlsSettings.NUM_PLUGIN_KEYS;
            m_Keys = new bool[countKeys];
            m_Flags = new ushort[countKeys];
            for (byte b = 0; b < countKeys; b++)
            {
                m_Flags[b] = (ushort)(1 << b);
            }

            Player.onPlayerTeleported += OnPlayerTeleported;
        }

        private void OnPlayerTeleported(Player player, Vector3 point)
        {
            m_OldPosition = point;

            /*if (m_Packet is WalkingPlayerInputPacket walking)
            {
                walking.clientPosition = point;
            }*/
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

        /* Simulate (0) [m_Count % PlayerInput.SAMPLES == 0]
         * 3 FU 
         * Send (3) [m_Consumed == m_Buffer]
         * Simulate (4)
         * 3 FU
         * Send (7)
         * ...
         */

        public async UniTaskVoid Start()
        {
            await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);
            var queue = (Queue<PlayerInputPacket>)s_ServerSidePacketsField.GetValue(Player.input);

            while (Enabled)
            {
                // Do not simulate if dead
                if (Player.life.isDead)
                    continue;

                if (m_Count % PlayerInput.SAMPLES == 0)
                {
                    SimulateAsClient();
                }

                if (m_Consumed < m_Buffer)
                {
                    m_Consumed++;
                }

                if (m_Consumed == m_Buffer)
                {
                    ushort compressedKeys = 0;
                    for (var b = 0; b < m_Keys.Length && m_Keys[b]; b++)
                    {
                        compressedKeys |= m_Flags[b];
                    }
                    m_Packet.keys = compressedKeys;

                    if (m_Packet is DrivingPlayerInputPacket drivingPlayerInputPacket)
                    {
                        var vehicle = Player.movement.getVehicle();

                        if (vehicle != null)
                        {
                            var transform = vehicle.transform;
                            drivingPlayerInputPacket.position = vehicle.asset.engine is EEngine.TRAIN
                                ? Patch_InteractableVehicle.PackRoadPositionOriginal(vehicle.roadPosition)
                                : vehicle.transform.position;

                            drivingPlayerInputPacket.rotation = transform.rotation;

                            drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                            drivingPlayerInputPacket.physicsSpeed = (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                            drivingPlayerInputPacket.turn = (byte)(Move.x + 1);
                        }
                    }

                    queue.Enqueue(m_Packet);
                }

                m_Count++;
                await UniTask.WaitForFixedUpdate();
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

        public ValueTask DisposeAsync()
        {
            Enabled = false;

            Player.onPlayerTeleported -= OnPlayerTeleported;
            return new();
        }
    }
}
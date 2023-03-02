using SDG.Framework.Water;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Threads;

public partial class DummyUserSimulationThread
{
    private Vector3 m_Velocity;
    private Vector3 m_OldPosition;

    private float fall => m_Velocity.y;

    public void SimulateAsClient()
    {
        var movement = Player.movement;
        var transform = Player.transform;
        var normalizedMove = Move.normalized;
        var speed = movement.speed;
        var deltaTime = PlayerInput.RATE;
        var stance = Player.stance.stance;
        var controller = movement.controller;
        var aim = Player.look.aim;

        if (Player.input.clientHasPendingResimulation)
        {
            Player.input.clientHasPendingResimulation = false;

            controller.enabled = false;
            transform.position = Player.input.clientResimulationPosition;
            controller.enabled = true;

            m_Velocity = Player.input.clientResimulationVelocity;

            m_OldPosition = transform.position;
            goto ForceCreatePacket;
        }

        m_OldPosition = transform.position;

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
                m_Velocity = transform.rotation * normalizedMove * speed * 1.5f;
                m_Velocity.y = (surfaceElevation - 1.275f - transform.position.y) / 8f;
                controller.CheckedMove(m_Velocity * deltaTime);

                break;
            default:
                var isMovementBlocked = false;
                var shouldUpdateVelocity = false;

                movement.checkGround(transform.position);

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
                        m_Velocity += a * 16f * deltaTime;
                        shouldUpdateVelocity = true;
                    }
                }

                if (!isMovementBlocked)
                {
                    var moveVector = transform.rotation * (normalizedMove * speed);

                    if (movement.isGrounded)
                    {
                        var characterFrictionProperties = PhysicMaterialCustomData.GetCharacterFrictionProperties(movement.materialName);
                        if (characterFrictionProperties.mode == EPhysicsMaterialCharacterFrictionMode.ImmediatelyResponsive)
                        {
                            moveVector = Vector3.Cross(Vector3.Cross(Vector3.up, moveVector), movement.ground.normal);
                            moveVector.y = Mathf.Min(moveVector.y, 0f);
                            m_Velocity = moveVector;
                        }
                        else
                        {
                            Vector3 vector3 = Vector3.ProjectOnPlane(m_Velocity, movement.ground.normal);
                            float magnitude = vector3.magnitude;
                            Vector3 vector4 = Vector3.Cross(Vector3.Cross(Vector3.up, moveVector), movement.ground.normal);
                            vector4 *= characterFrictionProperties.maxSpeedMultiplier;
                            float magnitude2 = vector4.magnitude;
                            float num7;
                            if (magnitude > magnitude2)
                            {
                                float num6 = -2f * characterFrictionProperties.decelerationMultiplier;
                                num7 = Mathf.Max(magnitude2, magnitude + (num6 * deltaTime));
                            }
                            else
                            {
                                num7 = magnitude2;
                            }
                            Vector3 vector5 = vector4 * characterFrictionProperties.accelerationMultiplier;
                            Vector3 vector6 = vector3 + (vector5 * deltaTime);
                            m_Velocity = vector6.ClampMagnitude(num7);
                            shouldUpdateVelocity = true;
                        }
                    }
                    else
                    {
                        m_Velocity.y += Physics.gravity.y * ((fall <= 0f) ? movement.totalGravityMultiplier : 1f) * deltaTime * 3f;
                        var maxFall = (movement.totalGravityMultiplier < 0.99f) ? (Physics.gravity.y * 2f * movement.totalGravityMultiplier) : (-100f);
                        m_Velocity.y = Mathf.Max(maxFall, m_Velocity.y);

                        var moveHorizontalMagnitude = moveVector.GetHorizontalMagnitude();
                        var horizontal = m_Velocity.GetHorizontal();
                        var horizontalMagnitude2 = m_Velocity.GetHorizontalMagnitude();
                        float maxMagnitude;
                        if (horizontalMagnitude2 > moveHorizontalMagnitude)
                        {
                            var num5 = 2f * Provider.modeConfigData.Gameplay.AirStrafing_Deceleration_Multiplier;
                            maxMagnitude = Mathf.Max(moveHorizontalMagnitude, horizontalMagnitude2 - (num5 * deltaTime));
                        }
                        else
                        {
                            maxMagnitude = moveHorizontalMagnitude;
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
                if (Jump
                    && movement.isGrounded
                    && !Player.life.isBroken
                    && Player.life.stamina >= 10f * (1f - (jumpMastery * 0.5f))
                    && stance is EPlayerStance.STAND or EPlayerStance.SPRINT
                    && !MathfEx.IsNearlyZero(movement.pluginJumpMultiplier, 0.001f))
                {
                    m_Velocity.y = c_Jump * (1f + (jumpMastery * 0.25f)) * movement.pluginJumpMultiplier;
                }

                m_Velocity += movement.pendingLaunchVelocity;
                movement.pendingLaunchVelocity = Vector3.zero;

                var previousPosition = movement.transform.position;
                controller.CheckedMove(m_Velocity * deltaTime);

                if (shouldUpdateVelocity)
                {
                    m_Velocity = (transform.position - previousPosition) / deltaTime;
                }

                break;
        }

        ForceCreatePacket:
        if (stance is EPlayerStance.DRIVING)
        {
            m_Packet = new DrivingPlayerInputPacket();
        }
        else
        {
            var x = (int)Move.x;
            var z = (int)Move.z;

            var horizontal = (byte)(x + 1);
            var vertical = (byte)(z + 1);

            m_Packet = new WalkingPlayerInputPacket
            {
                analog = (byte)((horizontal << 4) | vertical),
                clientPosition = transform.position,
                pitch = Mathf.Lerp(Player.look.pitch, m_Pitch, m_TimeLerp),
                yaw = Mathf.Lerp(Player.look.yaw, m_Yaw, m_TimeLerp)
            };
        }

        m_Packet.clientSimulationFrameNumber = m_Simulation;
        m_Packet.recov = Player.input.recov;

        m_Simulation++;
        m_Buffer += PlayerInput.SAMPLES;

        transform.position = m_OldPosition;
    }
}
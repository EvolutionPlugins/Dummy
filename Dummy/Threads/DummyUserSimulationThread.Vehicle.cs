using SDG.Framework.Water;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Threads;

public partial class DummyUserSimulationThread
{
	private float m_Factor;
	private Rigidbody? m_Rigidbody;
	private Transform? m_Buoyancy;
	private float m_AltSpeedInput;
	private float m_AltSpeedOutput;
	private float m_SpeedTraction;

	private void SimulateVehicle()
	{
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
		m_Factor = vehicle.factor;
		m_SpeedTraction = vehicle.speedTraction;

		if (!m_Buoyancy)
		{
			m_Buoyancy = vehicle.buoyancy;
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
				vehicle.isBoosting = true;
			}
			else
			{
				vehicle.isBoosting = false;
				moveY *= asset.staminaBoost;
				speed *= asset.staminaBoost;
			}
		}
		else
		{
			vehicle.isBoosting = false;
		}

		vehicle._speed = 150f;
		vehicle.isPhysical = true;

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

		switch (asset.engine)
		{
			case EEngine.CAR:
				if (tireOnGround)
				{
					m_Rigidbody.AddForce(-vehicle.transform.up * m_Factor * 40f);
					m_Rigidbody.AddForce(vehicle.transform.forward * 20f);
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

		vehicle._speed = _speed;
		vehicle._physicsSpeed = _physicsSpeed;
		vehicle._factor = m_Factor;
		vehicle.speedTraction = m_SpeedTraction;
	}
}
using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
using OpenMod.Extensions.Games.Abstractions.Entities;
using OpenMod.Extensions.Games.Abstractions.Items;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Extensions.Games.Abstractions.Transforms;
using OpenMod.Extensions.Games.Abstractions.Vehicles;
using OpenMod.UnityEngine.Extensions;
using OpenMod.UnityEngine.Helpers;
using OpenMod.UnityEngine.Transforms;
using OpenMod.Unturned.Items;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using Steamworks;
using System;
using System.Numerics;
using System.Threading.Tasks;
using IHasInventory = OpenMod.Extensions.Games.Abstractions.Entities.IHasInventory;

namespace EvolutionPlugins.Dummy.Models.Players
{
    public class DummyPlayer : IEquatable<DummyUser>, IPlayer, IHasHealth, IHasInventory, ICanEnterVehicle, IDamageSource
    {
        public SteamPlayer SteamPlayer { get; }
        public Player Player { get; }
        public CSteamID SteamID { get; }

        public DummyPlayer(SteamPlayer steamPlayer)
        {
            SteamPlayer = steamPlayer;
            Player = steamPlayer.player;
            SteamID = steamPlayer.playerID.steamID;
            Asset = UnturnedPlayerAsset.Instance;
            State = NullEntityState.Instance;
            Inventory = new UnturnedPlayerInventory(Player);
            Transform = new UnityTransform(Player.transform);
            EntityInstanceId = SteamID.ToString();
        }

        public IVehicle CurrentVehicle
        {
            get
            {
                var vehicle = Player.movement.getVehicle();
                if (vehicle == null)
                {
                    return null;
                }

                return new UnturnedVehicle(vehicle);
            }
        }

        public bool IsAlive => !Player.life.isDead;

        public double MaxHealth => 100;

        public double Health => Player.life.health;

        public string Stance => Player.stance.stance.ToString();

        public IEntityAsset Asset { get; }

        public IEntityState State { get; }

        public string EntityInstanceId { get; }

        public IWorldTransform Transform { get; }

        public IInventory Inventory { get; }

        public Task DamageAsync(double amount)
        {
            async UniTask SetHealthTask()
            {
                await UniTask.SwitchToMainThread();
                DamageTool.damagePlayer(new DamagePlayerParameters
                {
                    player = Player,
                    times = 1,
                    damage = (float)amount
                }, out _);
            }

            return SetHealthTask().AsTask();
        }

        public Task KillAsync()
        {
            return DamageAsync(float.MaxValue);
        }

        public Task SetHealthAsync(double health)
        {
            async UniTask SetHealthTask()
            {
                await UniTask.SwitchToMainThread();
                Player.life.askHeal((byte)health, Player.life.isBleeding, Player.life.isBroken);
            }

            return SetHealthTask().AsTask();
        }

        public Task<bool> SetPositionAsync(Vector3 position)
        {
            return SetPositionAsync(position, Player.transform.rotation.eulerAngles.ToSystemVector());
        }

        public Task<bool> SetPositionAsync(Vector3 position, Vector3 rotation)
        {
            async UniTask<bool> TeleportationTask()
            {
                await UniTask.SwitchToMainThread();

                if (Player.transform.position == position.ToUnityVector() &&
                    Player.transform.rotation.eulerAngles == rotation.ToUnityVector())
                {
                    return true;
                }

                if (!ValidationHelper.IsValid(position) || !ValidationHelper.IsValid(rotation))
                {
                    return false;
                }

                var rotationAngle = MeasurementTool.angleToByte(rotation.Y);
                Player.channel.send("askTeleport", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, position, rotationAngle);
                return true;
            }

            return TeleportationTask().AsTask();
        }

        public bool Equals(DummyUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.SteamID.Equals(SteamID);
        }

        public override bool Equals(object obj)
        {
            if (obj is DummyPlayer other) return Equals(other);
            else return false;
        }

        public override int GetHashCode()
        {
            return -975366258 + SteamID.GetHashCode();
        }
    }
}

using System;
using Cysharp.Threading.Tasks;
using Dummy.Actions.Interaction.Actions.Vehicle;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Extensions.Games.Abstractions.Players;
using OpenMod.Extensions.Games.Abstractions.Vehicles;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Vehicles;
using SDG.Framework.Utilities;
using SDG.Unturned;

namespace Dummy.Commands.Actions
{
    [Command("ride")]
    [CommandParent(typeof(CommandDummy))]
    [CommandSyntax("[--exit]")]
    public class CommandDummyRide : CommandDummyAction
    {
        public CommandDummyRide(IServiceProvider serviceProvider, IDummyProvider dummyProvider, IStringLocalizer stringLocalizer)
            : base(serviceProvider, dummyProvider, stringLocalizer)
        {
        }

        protected override UniTask ExecuteDummyAsync(DummyUser playerDummy)
        {
            var exitText = Context.Parameters.Count >= 2 ? Context.Parameters[1].ToLower() : null;
            var isExit = exitText switch
            {
                "exit" or "e" or "--exit" or "-e" => true,
                _ => false
            };

            if (isExit)
            {
                playerDummy.Actions.Actions.Enqueue(new ExitVehicleAction());
                return UniTask.CompletedTask;
            }

            InteractableVehicle? vehicle1 = null;

            if (Context.Actor is IPlayerUser playerUser)
            {
                if (playerUser.Player is ICanEnterVehicle vehicleActor && vehicleActor.CurrentVehicle is UnturnedVehicle vehicle)
                {
                    vehicle1 = vehicle.Vehicle;
                }

                if (vehicle1 == null && playerUser is UnturnedUser unturnedUser)
                {
                    var aim = unturnedUser.Player.Player.look.aim;
                    PhysicsUtility.raycast(new(aim.position, aim.forward), out var hit, 8f, RayMasks.VEHICLE);
                    if (hit.transform != null)
                    {
                        vehicle1 = DamageTool.getVehicle(hit.transform);
                    }
                }
            }

            if (vehicle1 == null)
            {
                throw new UserFriendlyException("You're not in a vehicle or not look at it");
            }

            playerDummy.Actions.Actions.Enqueue(new EnterVehicleAction(vehicle1));

            return UniTask.CompletedTask;
        }
    }
}

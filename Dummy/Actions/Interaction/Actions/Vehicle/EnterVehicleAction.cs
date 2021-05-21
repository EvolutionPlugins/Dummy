extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using Dummy.Extensions;

namespace Dummy.Actions.Interaction.Actions.Vehicle
{
    [UsedImplicitly]
    public class EnterVehicleAction : IAction
    {
        public InteractableVehicle InteractableVehicle { get; }

        public EnterVehicleAction(InteractableVehicle interactableVehicle)
        {
            InteractableVehicle = interactableVehicle;
        }

        public EnterVehicleAction(uint instanceId)
        {
            InteractableVehicle = VehicleManager.findVehicleByNetInstanceID(instanceId);
        }

        public Task Do(DummyUser dummy)
        {
            async UniTask ForceEnter()
            {
                await UniTask.SwitchToMainThread();
                var context = dummy.GetContext();

                VehicleManager.ReceiveEnterVehicleRequest(in context, InteractableVehicle.instanceID,
                    InteractableVehicle.asset.hash, (byte)InteractableVehicle.asset.engine);
            }

            return ForceEnter().AsTask();
        }
    }
}
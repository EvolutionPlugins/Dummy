using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Extensions.Interaction.Actions.Vehicle
{
    public class ExitVehicleAction : IInteractionAction
    {
        private readonly InteractableVehicle m_InteractableVehicle;
        public uint InstanceId { get; }

        public ExitVehicleAction(uint instanceId)
        {
            InstanceId = instanceId;
            m_InteractableVehicle = VehicleManager.findVehicleByNetInstanceID(instanceId)
                ?? throw new ArgumentException(nameof(m_InteractableVehicle));
        }

        public ExitVehicleAction(InteractableVehicle interactableVehicle)
        {
            InstanceId = interactableVehicle.instanceID;
        }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            VehicleManager.instance.askExitVehicle(dummy.SteamID, Vector3.zero);
        }
    }
}

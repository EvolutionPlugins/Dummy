using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions.Vehicle
{
    public class EnterVehicleAction : IInteractionAction
    {
        private readonly InteractableVehicle m_InteractableVehicle;
        public uint InstanceId { get; }

        public EnterVehicleAction(InteractableVehicle interactableVehicle)
        {
            InstanceId = interactableVehicle.instanceID;
            m_InteractableVehicle = interactableVehicle;
        }

        public EnterVehicleAction(uint instanceId)
        {
            InstanceId = instanceId;
            m_InteractableVehicle = VehicleManager.findVehicleByNetInstanceID(instanceId);
        }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            VehicleManager.instance.askEnterVehicle(dummy.SteamID, InstanceId, m_InteractableVehicle.asset.hash,
                (byte)m_InteractableVehicle.asset.engine);
        }
    }
}

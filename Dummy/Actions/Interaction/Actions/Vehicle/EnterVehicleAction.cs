using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions.Vehicle
{
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

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            VehicleManager.instance.askEnterVehicle(dummy.SteamID, InteractableVehicle.instanceID, InteractableVehicle.asset.hash,
                (byte)InteractableVehicle.asset.engine);
        }
    }
}

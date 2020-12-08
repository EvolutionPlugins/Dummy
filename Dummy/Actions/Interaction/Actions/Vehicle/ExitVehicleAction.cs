using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Actions.Interaction.Actions.Vehicle
{
    public class ExitVehicleAction : IAction
    {
        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            VehicleManager.instance.askExitVehicle(dummy.SteamID, Vector3.zero);
        }
    }
}

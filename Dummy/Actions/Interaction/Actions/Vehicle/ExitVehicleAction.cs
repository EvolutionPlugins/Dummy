﻿extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using System.Threading.Tasks;
using Dummy.Extensions;
using UnityEngine;

namespace Dummy.Actions.Interaction.Actions.Vehicle
{
    [UsedImplicitly]
    public class ExitVehicleAction : IAction
    {
        public Task Do(DummyUser dummy)
        {
            async UniTask ExitVehicle()
            {
                await UniTask.SwitchToMainThread();
                var context = dummy.GetContext();
                
                VehicleManager.ReceiveExitVehicleRequest(in context, Vector3.down);
            }

            return ExitVehicle().AsTask();
        }
    }
}
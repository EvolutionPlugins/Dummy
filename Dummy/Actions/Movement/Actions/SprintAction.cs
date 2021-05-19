extern alias JetBrainsAnnotations;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    [UsedImplicitly]
    public class SprintAction : IAction
    {
        public async Task Do(DummyUser dummy)
        {
            dummy.Simulation.Sprint = true;

            await UniTask.DelayFrame(5, PlayerLoopTiming.FixedUpdate);
            dummy.Simulation.Sprint = false;
        }
    }
}
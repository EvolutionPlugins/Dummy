using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using HarmonyLib;
using SDG.Unturned;

namespace Dummy.Actions.Movement.Actions
{
    public class JumpAction : IAction
    {
        public async Task Do(DummyUser dummy)
        {
            dummy.Simulation.Jump = true;

            await UniTask.DelayFrame(5, PlayerLoopTiming.FixedUpdate);
            dummy.Simulation.Jump = false;
        }
    }
}
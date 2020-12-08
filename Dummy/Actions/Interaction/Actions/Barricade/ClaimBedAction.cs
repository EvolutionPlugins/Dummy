using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Extensions.Interaction.Actions.Barricade
{
    public class ClaimBedAction : IAction
    {
        public ClaimBedAction(byte x, byte y, ushort plant, ushort index)
        {
            X = x;
            Y = y;
            Plant = plant;
            Index = index;
        }

        public ClaimBedAction(Transform barricade)
        {
            if (BarricadeManager.tryGetInfo(barricade, out var x, out var y, out var plant, out var index, out _))
            {
                X = x;
                Y = y;
                Plant = plant;
                Index = index;
            }
        }

        public byte X { get; }
        public byte Y { get; }
        public ushort Plant { get; }
        public ushort Index { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            BarricadeManager.instance.askClaimBed(dummy.SteamID, X, Y, Plant, Index);
        }
    }
}

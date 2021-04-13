extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Extensions;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Actions.Interaction.Actions.Barricade
{
    [UsedImplicitly]
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
            if (!BarricadeManager.tryGetInfo(barricade, out var x, out var y, out var plant,
                out var index, out _))
            {
                throw new Exception("Barricade cannot be founded!");
            }

            X = x;
            Y = y;
            Plant = plant;
            Index = index;
        }

        public byte X { get; }
        public byte Y { get; }
        public ushort Plant { get; }
        public ushort Index { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask ClaimBed()
            {
                await UniTask.SwitchToMainThread();

                var context = ServerInvocationContextExtenison.GetContext(dummy.SteamPlayer);

                BarricadeManager.ReceiveClaimBedRequest(context, X, Y, Plant, Index);
            }

            return ClaimBed().AsTask();
        }
    }
}
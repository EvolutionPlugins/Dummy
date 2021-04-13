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
    public class SalvageBarricadeAction : IAction
    {
        public SalvageBarricadeAction(byte x, byte y, ushort plant, ushort index)
        {
            X = x;
            Y = y;
            Plant = plant;
            Index = index;
        }

        public SalvageBarricadeAction(Transform barricade)
        {
            if (!BarricadeManager.tryGetInfo(barricade, out var x, out var y, out var plant,
                out var index, out _))
            {
                throw new Exception("Barricade not found");
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
            async UniTask SalvageBarricade()
            {
                await UniTask.SwitchToMainThread();

                var context = ServerInvocationContextExtenison.GetContext(dummy.SteamPlayer);

                BarricadeManager.ReceiveSalvageBarricadeRequest(context, X, Y, Plant, Index);
            }

            return SalvageBarricade().AsTask();
        }
    }
}
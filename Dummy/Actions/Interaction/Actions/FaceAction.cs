using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    public class FaceAction : IAction
    {
        public FaceAction(byte index)
        {
            // 32 index face is an empty face (very scary)
            if (index > Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            Index = index;
        }

        public byte Index { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask SetFace()
            {
                await UniTask.SwitchToMainThread();
                // todo: bypass Nelson index checking
                dummy.Player.Player.clothing.ReceiveSwapFaceRequest(Index);
            }

            return SetFace().AsTask();
        }
    }
}

using Cysharp.Threading.Tasks;
using Dummy.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

namespace Dummy.Extensions.Interaction.Actions
{
    public class FaceAction : IInteractionAction
    {
        public FaceAction(byte index)
        {
            if (index >= Customization.FACES_FREE + Customization.FACES_PRO)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            Index = index;
        }

        public byte Index { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Player.Player.equipment.channel.send("tellSwapFace", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                Index
            });
        }
    }
}

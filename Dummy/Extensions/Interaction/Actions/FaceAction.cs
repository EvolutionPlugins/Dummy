using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Extensions.Interaction.Actions
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

        public async Task Do(PlayerDummy dummy)
        {
            await UniTask.SwitchToMainThread();
            dummy.Data.UnturnedUser.Player.Player.equipment.channel.send("tellSwapFace", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
            {
                Index
            });
        }
    }
}

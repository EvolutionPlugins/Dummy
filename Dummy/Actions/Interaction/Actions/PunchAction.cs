extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Dummy.Actions.Interaction.Actions
{
    [UsedImplicitly]
    public class PunchAction : IAction
    {
        public PunchAction(EPlayerPunch punch)
        {
            Punch = punch;
        }

        public EPlayerPunch Punch { get; }

        public Task Do(DummyUser dummy)
        {
            async UniTask Punch()
            {
                await UniTask.SwitchToMainThread();
                // todo: add punch gesture simulation
                var aim = dummy.Player.Player.look.aim;

                var raycastInfo = DamageTool.raycast(new(aim.position, aim.forward), 1.75f,
                    RayMasks.DAMAGE_SERVER, dummy.Player.Player);
                dummy.Player.Player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Punch);
            }

            return Punch().AsTask();
        }
    }
}
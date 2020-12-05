using Cysharp.Threading.Tasks;
using Dummy.Extensions.Interaction.Actions;
using Dummy.Users;
using SDG.Unturned;
using System.Threading.Tasks;
using UnityEngine;

namespace Dummy.Extensions.Interaction
{
    public class PunchAction : IInteractionAction
    {
        public PunchAction(EPlayerPunch ePlayerPunch)
        {
            EPlayerPunch = ePlayerPunch;
        }

        public EPlayerPunch EPlayerPunch { get; }

        public async Task Do(DummyUser dummy)
        {
            await UniTask.SwitchToMainThread();

            var raycastInfo = DamageTool.raycast(new Ray(dummy.Player.Player.look.aim.position, dummy.Player.Player.look.aim.forward),
                1.75f, RayMasks.DAMAGE_SERVER, dummy.Player.Player);
            dummy.Player.Player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Punch);
        }
    }
}

using Cysharp.Threading.Tasks;
using Dummy.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Extensions
{
    public static class UnturnedExtension
    {
        public static async UniTask<bool> TeleportToLocationAsync(this DummyUser dummyUser, Vector3 position, float rotation)
        {
            await UniTask.SwitchToMainThread();
            var b = MeasurementTool.angleToByte(rotation);
            dummyUser.Player.Player.ReceiveTeleport(position, b);
            return true;
        }

        public static async UniTask<bool> TeleportToPlayerAsync(this DummyUser from, UnturnedUser to)
        {
            await UniTask.SwitchToMainThread();

            var transform = to.Player.Player.transform;

            return await from.TeleportToLocationAsync(transform.position, transform.rotation.eulerAngles.y);
        }
    }
}

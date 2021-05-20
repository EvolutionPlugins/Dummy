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

        public static UniTask<bool> TeleportToLocationAsync(this DummyUser dummyUser, Vector3 position) =>
             dummyUser.TeleportToLocationAsync(position, dummyUser.Player.Player.transform.eulerAngles.y);

        public static UniTask<bool> TeleportToPlayerAsync(this DummyUser from, UnturnedUser to)
        {
            Transform transform;
            return from.TeleportToLocationAsync((transform = to.Player.Player.transform).position, transform.rotation.eulerAngles.y);
        }
    }
}

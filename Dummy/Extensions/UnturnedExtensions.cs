using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace Dummy.Extensions
{
    public static class UnturnedExtensions
    {
        public static async UniTask<bool> TeleportToLocationAsync(this UnturnedUser unturnedUser, Vector3 position, float rotation)
        {
            await UniTask.SwitchToMainThread();
            return unturnedUser.Player.teleportToLocation(position, rotation);
        }

        public static async UniTask<bool> TeleportToLocationAsync(this UnturnedUser unturnedUser, Vector3 position) =>
            await TeleportToLocationAsync(unturnedUser, position, unturnedUser.Player.transform.eulerAngles.y);

        public static async UniTask<bool> TeleportToPlayerAsync(this UnturnedUser from, UnturnedUser to) =>
            await TeleportToLocationAsync(from, to.Player.transform.position, to.Player.transform.rotation.eulerAngles.y);
    }
}

using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Extensions
{
    public static class UnturnedExtensions
    {
        public static async UniTask<bool> TeleportToLocationAsync(this UnturnedUser unturnedUser, Vector3 position, float rotation)
        {
            await UniTask.SwitchToMainThread();
            return unturnedUser.Player.Player.teleportToLocation(position, rotation);
        }

        public static async UniTask<bool> TeleportToLocationAsync(this UnturnedUser unturnedUser, Vector3 position) =>
            await unturnedUser.TeleportToLocationAsync(position, unturnedUser.Player.Player.transform.eulerAngles.y);

        public static async UniTask<bool> TeleportToPlayerAsync(this UnturnedUser from, UnturnedUser to) =>
            await from.TeleportToLocationAsync(to.Player.Player.transform.position, to.Player.Player.transform.rotation.eulerAngles.y);
    }
}

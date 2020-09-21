using Cysharp.Threading.Tasks;
using EvolutionPlugins.Dummy.Models.Users;
using OpenMod.Unturned.Users;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Extensions
{
    public static class UnturnedExtensions
    {
        public static async UniTask<bool> TeleportToLocationAsync(this DummyUser DummyUser, Vector3 position, float rotation)
        {
            await UniTask.SwitchToMainThread();
            return DummyUser.Player.Player.teleportToLocation(position, rotation);
        }

        public static async UniTask<bool> TeleportToLocationAsync(this DummyUser DummyUser, Vector3 position) =>
            await DummyUser.TeleportToLocationAsync(position, DummyUser.Player.Player.transform.eulerAngles.y);

        public static async UniTask<bool> TeleportToPlayerAsync(this DummyUser from, UnturnedUser to) =>
            await from.TeleportToLocationAsync(to.Player.Player.transform.position, to.Player.Player.transform.rotation.eulerAngles.y);
    }
}

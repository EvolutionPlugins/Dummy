using Cysharp.Threading.Tasks;
using Dummy.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Extensions
{
    public static class UnturnedExtension
    {
        public static async UniTask<bool> TeleportToLocationAsync(this DummyUser DummyUser, Vector3 position, float rotation)
        {
            await UniTask.SwitchToMainThread();
            var b = MeasurementTool.angleToByte(rotation);
            DummyUser.Player.Player.channel.send("askTeleport", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, position, b);
            return true;
        }

        public static UniTask<bool> TeleportToLocationAsync(this DummyUser DummyUser, Vector3 position) =>
             DummyUser.TeleportToLocationAsync(position, DummyUser.Player.Player.transform.eulerAngles.y);

        public static UniTask<bool> TeleportToPlayerAsync(this DummyUser from, UnturnedUser to) =>
             from.TeleportToLocationAsync(to.Player.Player.transform.position, to.Player.Player.transform.rotation.eulerAngles.y);
    }
}

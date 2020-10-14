using Cysharp.Threading.Tasks;
using OpenMod.Core.Users;
using SDG.Unturned;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dummy.Users
{
    public class DummyUserSession : UserSessionBase
    {
        public DummyUserSession(DummyUser user) : base(user)
        {
            SessionData = new Dictionary<string, object>();
            SessionStartTime = DateTime.Now;
        }

        public override Task DisconnectAsync(string reason = "")
        {
            async UniTask Task()
            {
                await UniTask.SwitchToMainThread();
                SessionEndTime = DateTime.Now;
                RemoveDummy();
                foreach (SteamPlayer steamPlayer in Provider.clients)
                {
                    Provider.sendToClient(steamPlayer.transportConnection, ESteamPacket.DISCONNECTED, new byte[]
                    {
                        12,
                        (byte)((DummyUser)User).InternalIndex
                    }, 2);
                }
            }

            return Task().AsTask();
        }

        private void RemoveDummy()
        {
            var user = (DummyUser)User;
            if (user.SteamPlayer.model != null)
            {
                UnityEngine.Object.Destroy(user.SteamPlayer.model.gameObject);
            }
        }

        public void OnSessionEnd()
        {
            SessionEndTime = DateTime.Now;
        }
    }
}

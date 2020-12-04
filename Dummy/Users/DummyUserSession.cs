using Cysharp.Threading.Tasks;
using OpenMod.Core.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dummy.Users
{
    public class DummyUserSession : UserSessionBase
    {
        private static bool s_IsShuttingDown;

        static DummyUserSession()
        {
            Provider.onCommenceShutdown += Provider_onCommenceShutdown;
        }

        private static void Provider_onCommenceShutdown()
        {
            s_IsShuttingDown = true;
        }

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
                if (!s_IsShuttingDown)
                {
                    Provider.kick(((DummyUser)User).SteamID, reason ?? string.Empty);
                }
            }

            return Task().AsTask();
        }

        public void OnSessionEnd()
        {
            SessionEndTime = DateTime.Now;
        }
    }
}

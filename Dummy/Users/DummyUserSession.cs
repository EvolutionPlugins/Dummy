using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Users;
using SDG.Unturned;

namespace Dummy.Users
{
    public sealed class DummyUserSession : UserSessionBase
    {
        private static bool s_isShuttingDown;

        static DummyUserSession()
        {
            Provider.onCommenceShutdown += Provider_onCommenceShutdown;
        }

        private static void Provider_onCommenceShutdown()
        {
            s_isShuttingDown = true;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public DummyUserSession(DummyUser user) : base(user)
        {
            SessionData = new();
            SessionStartTime = DateTime.Now;
        }

        public override Task DisconnectAsync(string reason = "")
        {
            async UniTask Task()
            {
                await UniTask.SwitchToMainThread();
                SessionEndTime = DateTime.Now;
                if (!s_isShuttingDown)
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

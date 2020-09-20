using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Reflection;

namespace EvolutionPlugins.Dummy
{
    public static class Utils
    {
        #region SDG.Unturned methods
        public static void checkBanStatus(SteamPlayerID playerID, uint remoteIP, out bool isBanned, out string banReason, out uint banRemainingDuration)
        {
            isBanned = false;
            banReason = string.Empty;
            banRemainingDuration = 0U;

            // call original method if someone patch is with harmony
            var dynMethod = typeof(Provider).GetMethod("checkBanStatus", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            var parameters = new object[] { playerID, remoteIP, isBanned, banReason, banRemainingDuration };
            dynMethod.Invoke(typeof(Provider), parameters);

            isBanned = (bool)parameters[2];
            banReason = (string)parameters[3];
            banRemainingDuration = (uint)parameters[4];
        }

        public static void notifyClientPending(ITransportConnection transportConnection)
        {
            var dynMethod = typeof(Provider).GetMethod("notifyClientPending", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            dynMethod.Invoke(typeof(Provider), new[] { transportConnection });
        }

        public static void verifyNextPlayerInQueue()
        {
            var dynMethod = typeof(Provider).GetMethod("verifyNextPlayerInQueue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            dynMethod.Invoke(typeof(Provider), Array.Empty<object>());
        }
        #endregion
    }
}

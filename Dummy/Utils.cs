using SDG.Unturned;
using Steamworks;
using System;

namespace Dummy
{
    public static class Utils
    {
        public static void checkBanStatus(SteamPlayerID playerID, uint remoteIP, out bool isBanned, out string banReason,
            out uint banRemainingDuration)
        {
            isBanned = false;
            banReason = string.Empty;
            banRemainingDuration = 0U;
            if (SteamBlacklist.checkBanned(playerID.steamID, remoteIP, out SteamBlacklistID steamBlacklistID))
            {
                isBanned = true;
                banReason = steamBlacklistID.reason;
                banRemainingDuration = steamBlacklistID.getTime();
            }

            try
            {
                Provider.onCheckBanStatusWithHWID?.Invoke(playerID, remoteIP, ref isBanned, ref banReason, ref banRemainingDuration);

#pragma warning disable CS0612 // Type or member is obsolete
                Provider.onCheckBanStatus?.Invoke(playerID.steamID, remoteIP, ref isBanned, ref banReason, ref banRemainingDuration);
#pragma warning restore CS0612 // Type or member is obsolete
            }
            catch (Exception e)
            {
                UnturnedLog.warn("Plugin raised an exception from onCheckBanStatus:");
                UnturnedLog.exception(e);
            }
        }

        public static void notifyClientPending(CSteamID remoteSteamID)
        {
            byte[] bytes = SteamPacker.getBytes(0, out int size, 26);
            Provider.send(remoteSteamID, ESteamPacket.CLIENT_PENDING, bytes, size, 0);
        }

        public static void verifyNextPlayerInQueue()
        {
            if (Provider.pending.Count < 1)
            {
                return;
            }
            if (Provider.clients.Count - Dummy.Instance.Dummies.Count >= Provider.maxPlayers)
            {
                return;
            }
            SteamPending steamPending = Provider.pending[0];
            if (steamPending.hasSentVerifyPacket)
            {
                return;
            }
            steamPending.sendVerifyPacket();
        }
    }
}

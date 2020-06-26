using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(Provider), "verifyNextPlayerInQueue")]
    public static class Patch_Provider_verifyNextPlayerInQueue
    {
        public static bool Prefix()
        {
			if (Provider.pending.Count < 1)
			{
				return false;
			}
			if (Provider.clients.Count - Dummy.Instance.Dummies.Count >= Provider.maxPlayers)
			{
				return false;
			}
			SteamPending steamPending = Provider.pending[0];
			if (steamPending.hasSentVerifyPacket)
			{
				return false;
			}
			steamPending.sendVerifyPacket();
			return true;
		}
    }
}

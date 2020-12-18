using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dummy.Patches
{
#if DEBUG
    //[HarmonyPatch(typeof(PlayerMovement), "simulate")]
    public static class Patch_PlayerMovement
    {
        public static void Postfix(PlayerMovement __instance)
        {

        }
    }
#endif
}

using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dummy.Patches
{
    // ReSharper disable once InconsistentNaming
    internal static class Patch_ServerMessageHandler_ReadyToConnect
    {
        public static IEnumerable<CodeInstruction> ReadMessage(IEnumerable<CodeInstruction> instructions)
        {
            var providerMaxPlayers = typeof(Provider).GetProperty(nameof(Provider.maxPlayers), AccessTools.all)?.GetGetMethod()
                ?? throw new NullReferenceException("providerMaxPlayers is null");

            var codes = new List<CodeInstruction>(instructions);
            var index = codes.FindIndex(x => x.Calls(providerMaxPlayers));
            if (index < 0)
            {
                return codes;
            }

            // insert after that IL
            index++;

            codes.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Call, Patch_Provider.s_GetDummiesCount),
                new CodeInstruction(OpCodes.Sub),
            });

            return codes;
        }
    }
}

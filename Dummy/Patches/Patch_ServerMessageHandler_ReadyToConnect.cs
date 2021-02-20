using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Dummy.Patches
{
    public static class Patch_ServerMessageHandler_ReadyToConnect
    {
        public static IEnumerable<CodeInstruction> ReadMessage(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                if (instruction.opcode == OpCodes.Call && instruction.Calls(Patch_Provider.s_GetClients))
                {
                    i += 4;

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, Patch_Provider.s_GetDummiesCount));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Sub, null));
                    break;
                }
            }
            return codes;
        }
    }
}

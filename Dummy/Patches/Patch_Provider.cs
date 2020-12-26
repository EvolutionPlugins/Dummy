using HarmonyLib;
using SDG.Unturned;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(Provider))]
    public static class Patch_Provider
    {
        internal static event NeedDummyProvider OnNeedDummy;

        public static int GetDummiesCount()
        {
            return OnNeedDummy?.Invoke().Dummies.Count ?? 0;
        }

        private static readonly MethodInfo m_GetClients = AccessTools.DeclaredPropertyGetter(typeof(Provider), "clients");
        private static readonly MethodInfo m_GetDummiesCount = SymbolExtensions.GetMethodInfo(() => GetDummiesCount());

        [HarmonyTranspiler]
        [HarmonyPatch("receiveServer")]
#if DEBUG
        [HarmonyDebug]
#endif
        public static IEnumerable<CodeInstruction> receiveServer(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                if (instruction.opcode == OpCodes.Call && instruction.Calls(m_GetClients))
                {
                    i += 4;

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, m_GetDummiesCount));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Sub, null));
                    break;
                }
            }
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("verifyNextPlayerInQueue")]
#if DEBUG
        [HarmonyDebug]
#endif
        public static IEnumerable<CodeInstruction> verifyNextPlayerInQueue(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                if (instruction.opcode == OpCodes.Call && instruction.Calls(m_GetClients))
                {
                    i += 2;

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, m_GetDummiesCount));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Sub, null));
                    break;
                }
            }
            return codes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SDG.Unturned;
using Serilog;

namespace Dummy.Patches
{
    [HarmonyPatch(typeof(Provider))]
    internal static class Patch_Provider
    {
        internal static event NeedDummyProvider? OnNeedDummy;

        public static int GetDummiesCount()
        {
            return OnNeedDummy?.Invoke().Dummies.Count ?? 0;
        }

        [HarmonyCleanup]
        public static Exception? Cleanup(Exception exception)
        {
            Log.Error(exception, "Failed to patch\n{Stacktrace}", Environment.StackTrace);
            return null;
        }

        internal static readonly MethodInfo s_GetClients = AccessTools.DeclaredPropertyGetter(typeof(Provider), "clients");
        internal static readonly MethodInfo s_GetDummiesCount = SymbolExtensions.GetMethodInfo(() => GetDummiesCount());

        [HarmonyTranspiler]
        [HarmonyPatch("verifyNextPlayerInQueue")]
        public static IEnumerable<CodeInstruction> VerifyNextPlayerInQueue(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                var instruction = codes[i];
                if (instruction.opcode != OpCodes.Call || !instruction.Calls(s_GetClients))
                {
                    continue;
                }

                i += 2;

                codes.Insert(i, new(OpCodes.Call, s_GetDummiesCount));
                codes.Insert(i + 1, new(OpCodes.Sub));
                break;
            }
            return codes;
        }
    }
}

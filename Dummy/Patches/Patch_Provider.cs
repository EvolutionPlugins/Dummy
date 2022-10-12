extern alias JetBrainsAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrainsAnnotations::JetBrains.Annotations;
using SDG.Unturned;

namespace Dummy.Patches
{
    // ReSharper disable InconsistentNaming
    [HarmonyPatch(typeof(Provider))]
    internal static class Patch_Provider
    {
        internal static event NeedDummyProvider? OnNeedDummy;

        public static int GetDummiesCount()
        {
            return OnNeedDummy?.Invoke().Dummies.Count ?? 0;
        }

        internal static readonly MethodInfo s_GetClients = AccessTools.DeclaredPropertyGetter(typeof(Provider), "clients");
        internal static readonly MethodInfo s_GetDummiesCount = SymbolExtensions.GetMethodInfo(() => GetDummiesCount());

        [HarmonyPrefix]
        [HarmonyPatch("battlEyeServerKickPlayer")]
        [UsedImplicitly]
        public static bool battlEyeServerKickPlayer(int playerID)
        {
            return !(OnNeedDummy?.Invoke().Dummies.Any(x => x.Player.BattlEyeId == playerID) ?? false);
        }

        [HarmonyTranspiler]
        [HarmonyPatch("verifyNextPlayerInQueue")]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> verifyNextPlayerInQueue(IEnumerable<CodeInstruction> instructions)
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

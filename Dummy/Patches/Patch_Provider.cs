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
        internal static event NeedDummyProvider OnNeedProvider;
        public static int GetDummiesCount()
        {
            return OnNeedProvider?.Invoke().Dummies.Count ?? 0;
        }

        public static readonly MethodInfo get_Clients = AccessTools.DeclaredPropertyGetter(typeof(Provider), "clients");
        public static readonly MethodInfo get_DummiesCount = SymbolExtensions.GetMethodInfo(() => GetDummiesCount());

        // if someone is better know Harmony Transpiler please review my bad patching and rewrite if you can

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
                // IL_0774: call      class [mscorlib]System.Collections.Generic.List`1<class SDG.Unturned.SteamPlayer> SDG.Unturned.Provider::get_clients()
                if (instruction.opcode == OpCodes.Call && instruction.Calls(get_Clients))
                {
                    i += 4;

                    // How is should be after patching
                    //IL_0903: call       static System.Collections.Generic.List`1 < SDG.Unturned.SteamPlayer > SDG.Unturned.Provider::get_clients()
                    //IL_0908: callvirt   virtual System.Int32 System.Collections.Generic.List`1<SDG.Unturned.SteamPlayer>::get_Count()
                    //IL_090D: ldc.i4.1
                    //IL_090E: add
                    //IL_090F: call       static System.Int32 Dummy.Patches.Patch_Provider::GetDummiesCount() <<<
                    //IL_0914: sub <<<
                    //IL_0915: call static System.Byte SDG.Unturned.Provider::get_maxPlayers()
                    //IL_091A: ble =>     Label58 (exit)

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, get_DummiesCount));
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
                // IL_0774: call      class [mscorlib]System.Collections.Generic.List`1<class SDG.Unturned.SteamPlayer> SDG.Unturned.Provider::get_clients()
                if (instruction.opcode == OpCodes.Call && instruction.Calls(get_Clients))
                {
                    i += 2;
                    // now we after callvirt ::get_Count()

                    codes.Insert(i, new CodeInstruction(OpCodes.Call, get_DummiesCount));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Sub, null));
                    break;
                }
            }
            return codes;
        }
    }
}

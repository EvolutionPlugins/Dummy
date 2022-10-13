using System;
using HarmonyLib;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Patches;
[HarmonyPatch(typeof(InteractableVehicle))]
internal static class Patch_InteractableVehicle
{
    [HarmonyReversePatch]
    [HarmonyPatch("PackRoadPosition")]
    public static Vector3 PackRoadPositionOriginal(float roadPosition)
    {
        throw new NotImplementedException("It's a stub");
    }
}

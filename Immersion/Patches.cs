using Immersion.Components;
using HarmonyLib;

namespace Immersion;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Awake))]
    private static void OnCameraAwake(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraMovementController>();
        __instance.gameObject.AddComponent<ToolArmHandler>();
    }
}
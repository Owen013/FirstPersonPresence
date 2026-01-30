using HarmonyLib;

namespace Immersion.Components;

[HarmonyPatch]
public class CameraOffsetController : TransformOffsetController
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.UpdateCamera))]
    private static void PlayerCameraController_UpdateCamera_Prefix(PlayerCameraController __instance)
    {
        // remove camera offset before vanilla update logic
        var offsetController = __instance.GetComponent<CameraOffsetController>();
        offsetController?.ResetOffset();
    }
}
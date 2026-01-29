using HarmonyLib;

namespace Immersion.Components;

public class ToolOffsetController : TransformOffsetController
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.Update))]
    private static void PlayerTool_Update_Prefix(PlayerTool __instance)
    {
        // remove tool offset before vanilla update logic
        var offsetController = __instance.GetComponent<ToolOffsetController>();
        offsetController?.ResetOffset();
    }
}
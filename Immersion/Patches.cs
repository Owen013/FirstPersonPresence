using HarmonyLib;
using Immersion.Components;

namespace Immersion;

[HarmonyPatch]
internal static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void OWItem_PickUpItem_Postfix(OWItem __instance)
    {
        ViewmodelArm.OnPickUpItem(__instance);
        ItemUtils.OnPickUpItem(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
    private static void OWItem_DropItem_Postfix(OWItem __instance)
    {
        ViewmodelArm.OnDropItem(__instance);
        ItemUtils.OnDropItem(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void PlayerCameraController_Start_Postfix(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<OffsetManager>();
        __instance._playerCamera.nearClipPlane = Config.FixItemClipping ? 0.05f : 0.1f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance) =>
        ViewmodelArm.OnEquipTool(__instance);
}
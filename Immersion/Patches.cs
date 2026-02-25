using HarmonyLib;
using Immersion.Components;
using OWML.Utils;
using UnityEngine;

namespace Immersion;

[HarmonyPatch]
internal static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void OWItem_PickUpItem_Postfix(OWItem __instance)
    {
        ViewmodelArm.OnPickUpItem(__instance);
        if (__instance.GetItemType().GetName() == "GhostbirdSkull")
        {
            // TSTA skull renderer has weird bounds, so it can stop rendering when near the edges of the screen
            // normally isn't a problem, since its held position is not close enough to the edge of screen for this to be an issue
            // with Immersion installed, hand sway / hand height offset can cause skull to move far enough away to disappear
            // so set the renderers to update when "offscreen" while the skull is held
            foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
    private static void OWItem_DropItem_Postfix(OWItem __instance)
    {
        // return TSTA skull mesh renderers to normal when dropped
        if (__instance.GetItemType().GetName() == "GhostbirdSkull")
        {
            foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void PlayerCameraController_Start_Postfix(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<OffsetManager>();
        __instance._playerCamera.nearClipPlane = Config.FixViewmodelClipping ? 0.05f : 0.1f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance) =>
        ViewmodelArm.OnEquipTool(__instance);
}
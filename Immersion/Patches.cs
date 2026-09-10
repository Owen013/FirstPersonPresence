using HarmonyLib;
using Immersion.Components;
using OWML.Utils;
using UnityEngine;

namespace Immersion;

[HarmonyPatch]
static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    static void OWItem_PickUpItem_Postfix(OWItem __instance)
    {
        OffsetManager.OnPickUpItem(__instance);
        ViewmodelArm.OnPickUpItem(__instance);

        // Immersion can cause TSTA skull to be frustrum culled while still onscreen, so make it render offscreen
        // while held.
        if (__instance.GetItemType().GetName() == "GhostbirdSkull")
        {
            foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
    static void OWItem_DropItem_Postfix(OWItem __instance)
    {
        if (__instance.GetItemType().GetName() == "GhostbirdSkull")
        {
            foreach (var renderer in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    static void PlayerAnimController_LateUpdate_Postfix(PlayerAnimController __instance)
    {
        if (LandingAnimController.Instance != null)
            LandingAnimController.Instance.UpdateLandingCrouchAnim(__instance._animator);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    static void PlayerCameraController_Start_Postfix(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<OffsetManager>();
        __instance._playerCamera.nearClipPlane = Config.FixViewmodelClipping ? 0.05f : 0.1f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    static void PlayerTool_EquipTool_Postfix(PlayerTool __instance)
    {
        ViewmodelArm.OnEquipTool(__instance);
    }
}
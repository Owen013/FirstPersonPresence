using HarmonyLib;
using Immersion.Components;
using Immersion.Objects;
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
        // try to create a ViewmodelArm for the item
        if (Config.EnableViewmodelArms && ArmData.Exists(ArmData.TryGetArmDataID(__instance)) && __instance.transform.Find("ViewmodelArm") == null)
            ViewmodelArm.NewViewmodelArm(__instance);

        // TSTA skull renderer has weird bounds, so it can stop rendering when near the edges of the screen
        // normally isn't a problem, since its held position is not close enough to the edge of screen for this to be an issue
        // with Immersion installed, hand sway / hand height offset can cause skull to move far enough away to disappear
        // so set the renderers to update when "offscreen" while the skull is held
        if (__instance.GetItemType().GetName() == "GhostbirdSkull")
        {
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
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    private static void PlayerAnimController_LateUpdate_Postfix(PlayerAnimController __instance)
    {
        if (Config.UseLandingCrouchAnim && LandingAnimController.Instance != null)
            __instance._animator.SetLayerWeight(1, Mathf.Max(__instance._animator.GetLayerWeight(1), Mathf.Clamp01(LandingAnimController.Instance.LandingAnimPosition / -0.3f)));
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
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance)
    {
        // try to create a ViewmodelArm
        if (!Config.EnableViewmodelArms || !ArmData.Exists(__instance.name)) return;

        // check for existing arm and enable if found (PlayerTool has no event for being equipped, so this is required)
        var existingArm = __instance.transform.Find("ViewmodelArm");
        if (existingArm != null)
        {
            existingArm.gameObject.SetActive(true);
            return;
        }

        ViewmodelArm.NewViewmodelArm(__instance);
    }
}
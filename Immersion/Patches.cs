using Immersion.Components;
using HarmonyLib;
using UnityEngine;

namespace Immersion;

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void OnCameraAwake(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraMovementController>();
        ModMain.Instance.WriteLine($"{nameof(CameraMovementController)} added to {__instance.name}", OWML.Common.MessageType.Debug);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Start))]
    private static void OnAnimControllerStart(PlayerAnimController __instance)
    {
        __instance.gameObject.AddComponent<AnimSpeedController>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    private static void OnAnimControllerLateUpdate(PlayerAnimController __instance)
    {
        GameObject[] leftArmObjects =
        {
            __instance.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject,
            __instance.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject
        };

        for (int i = 0; i < __instance._rightArmObjects.Length; i++)
        {
            __instance._rightArmObjects[i].layer = __instance._defaultLayer;
        }
        for (int i = 0; i < leftArmObjects.Length; i++)
        {
            leftArmObjects[i].layer = __instance._defaultLayer;
        }

        ToolMode toolMode = Locator.GetToolModeSwapper().GetToolMode();
        __instance._rightArmHidden = toolMode > ToolMode.None;
        if (Config.IsLeftyModeEnabled && toolMode != ToolMode.Translator)
        {
            for (int i = 0; i < __instance._rightArmObjects.Length; i++)
            {
                leftArmObjects[i].layer = __instance._rightArmHidden ? __instance._probeOnlyLayer : __instance._defaultLayer;
            }
        }
        else
        {
            for (int i = 0; i < __instance._rightArmObjects.Length; i++)
            {
                __instance._rightArmObjects[i].layer = __instance._rightArmHidden ? __instance._probeOnlyLayer : __instance._defaultLayer;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.Update))]
    private static void OnItemToolUpdate(PlayerTool __instance)
    {
        if (__instance is not ItemTool) return;

        if (Config.IsHideStowedItemsEnabled && !__instance.IsEquipped() && !__instance.IsPuttingAway())
        {
            __instance.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
        }
    }
}
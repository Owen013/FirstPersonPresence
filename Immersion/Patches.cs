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
        __instance.gameObject.AddComponent<MovementController>();
        Main.Instance.WriteLine($"{nameof(MovementController)} added to {__instance.name}", OWML.Common.MessageType.Debug);
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
        if (__instance._rightArmHidden)
        {
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.Start))]
    private static void OnSignalscopeAwake(Signalscope __instance)
    {
        ToolArmHandler.NewToolArm(__instance.transform.Find("Props_HEA_Signalscope"), new Vector3(-0.01f, -0.11f, -0.16f), Quaternion.identity, new Vector3(0.3f, 0.3f, 0.3f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.Start))]
    private static void OnProbeLauncherAwake(ProbeLauncher __instance)
    {
        if (!__instance.GetComponentInParent<PlayerBody>()) return;
        ToolArmHandler.NewToolArm(__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis"), new Vector3(0.0556f, -0.5777f, 0.0957f), Quaternion.Euler(24.6841f, 0f, 0f), new Vector3(0.9f, 0.9f, 0.9f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.Start))]
    private static void OnTranslatorAwake(NomaiTranslatorProp __instance)
    {
        ToolArmHandler.NewToolArm(__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo"), new Vector3(0.6251f, -0.5804f, -0.2715f), Quaternion.identity, new Vector3(1.2f, 1.2f, 1.2f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SharedStone), nameof(SharedStone.Awake))]
    private static void OnSharedStoneAwake(SharedStone __instance)
    {
        ToolArmHandler.NewToolArm(__instance.transform.Find("AnimRoot/Props_NOM_SharedStone"), new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), new Vector3(0.9f, 0.9f, 0.9f), true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScrollItem), nameof(ScrollItem.Awake))]
    private static void OnScrollAwake(SharedStone __instance)
    {
        Vector3 position = __instance.name == "Prefab_NOM_Scroll_egg" ? new Vector3(-0.2028f, 0.0195f, -0.2974f) : new Vector3(-0.1748f, 0.0613f, -0.2957f);
        ToolArmHandler.NewToolArm(__instance.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo/"), position, Quaternion.Euler(358.7909f, 107.971f, 3.502f), new Vector3(0.9f, 0.9f, 0.9f), true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiConversationStone), nameof(NomaiConversationStone.Awake))]
    private static void OnSolanumStoneAwake(NomaiConversationStone __instance)
    {
        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.name.Contains("_Back"))
            {
                ToolArmHandler.NewToolArm(renderer.transform, new Vector3(0.1748f, -0.1398f, -0.2008f), Quaternion.Euler(0f, 0f, 292.1743f), new Vector3(0.6f, 0.6f, 0.6f), true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WarpCoreItem), nameof(WarpCoreItem.Awake))]
    private static void OnWarpCoreAwake(WarpCoreItem __instance)
    {
        if (__instance._warpCoreType == WarpCoreType.Vessel)
        {
            ToolArmHandler.NewToolArm(__instance.transform.Find("Props_NOM_WarpCore_Advanced/Props_NOM_WarpCore_Advance_Geo"), new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), new Vector3(0.9f, 0.9f, 0.9f), true);
        }
        else if (__instance._warpCoreType == WarpCoreType.VesselBroken)
        {
            ToolArmHandler.NewToolArm(__instance.transform.Find("Props_NOM_WarpCore_Advanced_Broken_V3/Props_NOM_WarpCore_Advance_Broken_Geo"), new Vector3(-0.2098f, -0.3825f, 0.0593f), Quaternion.Euler(8.5636f, 156.946f, 331.5615f), new Vector3(0.9f, 0.9f, 0.9f), true);
        }
        else
        {
            ToolArmHandler.NewToolArm(__instance.transform.Find("Props_NOM_WarpCore_Simple"), new Vector3(0.057f, -0.4437f, -0.4526f), Quaternion.Euler(323.3099f, 77.0467f, 330.0953f), new Vector3(2f, 2f, 2f), true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimpleLanternItem), nameof(SimpleLanternItem.Start))]
    private static void OnSimpleLanternAwake(SimpleLanternItem __instance)
    {
        ToolArm arm = ToolArmHandler.NewToolArm(__instance.transform.Find("Props_IP_Lantern/Lantern_geo"), new Vector3(-0.2524f, 0.2953f, -0.0524f), Quaternion.Euler(330f, 140f, 90f), new Vector3(1.2f, 1.2f, 1.2f), true);
        if (arm == null)
        {
            arm = ToolArmHandler.NewToolArm(__instance.transform.Find("Props_IP_Lantern_Crack/Lantern_geo"), new Vector3(0.2494f, 0.6859f, 0.0476f), Quaternion.Euler(330f, 320f, 90f), new Vector3(1.2f, 1.2f, 1.2f), true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SlideReelItem), nameof(SlideReelItem.Start))]
    private static void OnSlideReelAwake(SlideReelItem __instance)
    {
        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.name.Contains("Frame_"))
            {
                ToolArmHandler.NewToolArm(renderer.transform, new Vector3(-0.4143f, 0.1576f, -0.2241f), Quaternion.Euler(4.0031f, 145.1847f, 70.3509f), new Vector3(0.9f, 0.9f, 0.9f), true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.Start))]
    private static void OnDreamLanternAwake(DreamLanternItem __instance)
    {
        ToolArm arm = ToolArmHandler.NewToolArm(__instance.transform.Find("Props_IP_Artifact_ViewModel/artifact_geo"), new Vector3(0.1389f, 0.3851f, -0.144f), Quaternion.Euler(0f, 0f, 90f), new Vector3(1.2f, 1.2f, 1.2f));
        if (arm == null)
        {
            arm = ToolArmHandler.NewToolArm(__instance.transform.Find("ViewModel/Props_IP_DreamLanternItem_Malfunctioning (1)/PrototypeArtifact_2"), new Vector3(0.1389f, 0.836f, -0.144f), Quaternion.Euler(0f, 0f, 90f), new Vector3(1.2f, 1.2f, 1.2f));
            if (arm == null)
            {
                arm = ToolArmHandler.NewToolArm(__instance.transform.Find("Props_IP_DreamLanternItem_Nonfunctioning/PrototypeArtifact"), new Vector3(0.1593f, 0.7578f, -0.144f), Quaternion.Euler(330f, 0f, 90f), new Vector3(1.2f, 1.2f, 1.2f));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.Start))]
    private static void OnVisionTorchAwake(VisionTorchItem __instance)
    {
        ToolArmHandler.NewToolArm(__instance.transform.Find("Prefab_IP_VisionTorchProjector/Props_IP_ScannerStaff/Scannerstaff_geo"), new Vector3(0.0403f, 1.0224f, 0.141f), Quaternion.Euler(345.0329f, 184.0765f, 358.0521f), Vector3.one, true);
    }
}
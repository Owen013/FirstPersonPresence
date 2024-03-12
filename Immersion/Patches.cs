using Immersion.Components;
using HarmonyLib;
using UnityEngine;

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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.Awake))]
    private static void OnSignalscopeAwake(Signalscope __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("Props_HEA_Signalscope"), new Vector3(-0.01f, -0.11f, -0.16f), Quaternion.identity, new Vector3(0.3f, 0.3f, 0.3f))?.AddComponent<ToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.Awake))]
    private static void OnProbeLauncherAwake(ProbeLauncher __instance)
    {
        if (!__instance.GetComponentInParent<PlayerBody>()) return;
        ToolArmHandler.NewArm(__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis"), new Vector3(0.0615f, - 0.6004f, 0.0698f), Quaternion.identity, new Vector3(0.9f, 0.9f, 0.9f))?.AddComponent<ToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.Awake))]
    private static void OnTranslatorAwake(NomaiTranslatorProp __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo"), new Vector3(0.6342f, -0.5804f, -0.2f), Quaternion.identity, new Vector3(1.2f, 1.2f, 1.2f))?.AddComponent<ToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SharedStone), nameof(SharedStone.Awake))]
    private static void OnSharedStoneAwake(SharedStone __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("AnimRoot/Props_NOM_SharedStone"), new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), new Vector3(0.9f, 0.9f, 0.9f))?.AddComponent<ItemToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiConversationStone), nameof(NomaiConversationStone.Awake))]
    private static void OnSolanumStoneAwake(NomaiConversationStone __instance)
    {
        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.name.Contains("_Back"))
            {
                ToolArmHandler.NewArm(renderer.transform, new Vector3(0.1855f, -0.0942f, -0.184f), Quaternion.Euler(0f, 0f, 340.6367f), new Vector3(0.9f, 0.9f, 0.9f))?.AddComponent<ItemToolArm>();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScrollItem), nameof(ScrollItem.Awake))]
    private static void OnScrollAwake(SharedStone __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo/"), new Vector3(-0.1748f, -0.0246f, -0.1213f), Quaternion.Euler(358.7909f, 107.971f, 3.502f), new Vector3(0.9f, 0.9f, 0.9f))?.AddComponent<ItemToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimpleLanternItem), nameof(SimpleLanternItem.Awake))]
    private static void OnSimpleLanternAwake(SimpleLanternItem __instance)
    {
        GameObject arm = ToolArmHandler.NewArm(__instance.transform.Find("Props_IP_Lantern/Lantern_geo"), new Vector3(-0.2524f, 0.2953f, -0.0524f), Quaternion.Euler(330f, 140f, 90f), new Vector3(1.2f, 1.2f, 1.2f));
        if (arm == null)
        {
            arm = ToolArmHandler.NewArm(__instance.transform.Find("Props_IP_Lantern_Crack/Lantern_geo"), new Vector3(0.2494f, 0.6859f, 0.0476f), Quaternion.Euler(330f, 320f, 90f), new Vector3(1.2f, 1.2f, 1.2f));
        }
        arm?.AddComponent<ItemToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SlideReelItem), nameof(SlideReelItem.Awake))]
    private static void OnSlideReelAwake(SlideReelItem __instance)
    {
        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.name.Contains("Frame_"))
            {
                ToolArmHandler.NewArm(renderer.transform, new Vector3(-0.3298f, 0.121f, 0.133f), Quaternion.Euler(358.9547f, 87.493f, 84.6117f), new Vector3(0.9f, 0.9f, 0.9f))?.AddComponent<ItemToolArm>();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.Awake))]
    private static void OnDreamLanternAwake(DreamLanternItem __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("Props_IP_Artifact_ViewModel/artifact_geo"), new Vector3(0.1389f, 0.3851f, -0.144f), Quaternion.Euler(0f, 0f, 90f), new Vector3(1.2f, 1.2f, 1.2f))?.AddComponent<ItemToolArm>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.Awake))]
    private static void OnVisionTorchAwake(VisionTorchItem __instance)
    {
        ToolArmHandler.NewArm(__instance.transform.Find("Prefab_IP_VisionTorchProjector/Props_IP_ScannerStaff/Scannerstaff_geo"), new Vector3(0.0403f, 1.0224f, 0.141f), Quaternion.Euler(345.0329f, 184.0765f, 358.0521f), Vector3.one)?.AddComponent<ItemToolArm>();
    }
}
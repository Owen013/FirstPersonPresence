using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    private bool _isItem;

    private GameObject _noSuitArm;

    private GameObject _suitArm;

    private GameObject _realNoSuitRightArm;

    private GameObject _realSuitRightArm;

    private GameObject _realNoSuitLeftArm;

    private GameObject _realSuitLeftArm;

    public static ViewmodelArm Create(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false)
    {
        if (parent == null)
        {
            ModMain.Print($"Can't create viewmodel arm; parent is null", OWML.Common.MessageType.Debug);
            return null;
        }

        if (parent.GetComponent<ViewmodelArm>() != null)
        {
            ModMain.Print($"{parent.name} already has a viewmodel arm. Replacing it.", OWML.Common.MessageType.Debug);
            Destroy(parent.GetComponent<ViewmodelArm>().gameObject);
        }

        GameObject arm = new GameObject("ViewmodelArm");
        arm.transform.parent = parent;
        arm.transform.localPosition = localPos;
        arm.transform.localRotation = localRot;
        arm.transform.localScale = scale;

        GameObject noSuit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        noSuit.name = "Arm_NoSuit";
        noSuit.transform.parent = arm.transform;
        noSuit.layer = 27;
        noSuit.transform.localPosition = Vector3.zero;
        noSuit.transform.localRotation = Quaternion.Euler(330, 0, 300);
        noSuit.transform.localScale = Vector3.one;

        GameObject suit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        suit.name = "Arm_Suit";
        suit.transform.parent = arm.transform;
        suit.layer = 27;
        suit.transform.localPosition = new Vector3(-0.02f, 0.03f, 0.02f);
        suit.transform.localRotation = Quaternion.Euler(330, 0, 300);
        suit.transform.localScale = Vector3.one;

        MeshRenderer noSuitMeshRenderer = noSuit.GetComponent<MeshRenderer>();
        MeshRenderer suitMeshRenderer = suit.GetComponent<MeshRenderer>();
        noSuitMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        suitMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        foreach (Material material in noSuitMeshRenderer.materials)
        {
            material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
            if (!useDefaultShader)
            {
                material.shader = parent.GetComponent<MeshRenderer>().material.shader;
            }
        }

        suitMeshRenderer.material.renderQueue = noSuitMeshRenderer.material.renderQueue;
        suitMeshRenderer.material.shader = noSuitMeshRenderer.material.shader;

        return arm.AddComponent<ViewmodelArm>();
    }

    private void Start()
    {
        if (gameObject.GetComponentInParent<OWItem>() != null)
        {
            _isItem = true;
        }
        else
        {
            _isItem = false;
        }

        _noSuitArm = gameObject.transform.Find("Arm_NoSuit").gameObject;
        _suitArm = gameObject.transform.Find("Arm_Suit").gameObject;

        PlayerAnimController _playerVisuals = Locator.GetPlayerController().GetComponentInChildren<PlayerAnimController>();
        _realNoSuitRightArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _realSuitRightArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        _realNoSuitLeftArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
        _realSuitLeftArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;
    }

    private void Update()
    {
        if (!Config.IsViewModelHandsEnabled || (_isItem && !GetComponentInParent<ItemTool>()))
        {
            _noSuitArm.SetActive(false);
            _suitArm.SetActive(false);
        }
        else if (Config.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
        {
            _noSuitArm.SetActive(_realNoSuitLeftArm.activeInHierarchy);
            _suitArm.SetActive(_realSuitLeftArm.activeInHierarchy);
        }
        else
        {
            _noSuitArm.SetActive(_realNoSuitRightArm.activeInHierarchy);
            _suitArm.SetActive(_realSuitRightArm.activeInHierarchy);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.Start))]
    private static void OnSignalscopeAwake(Signalscope __instance)
    {
        Create(__instance.transform.Find("Props_HEA_Signalscope"), new Vector3(-0.01f, -0.11f, -0.16f), Quaternion.identity, new Vector3(0.3f, 0.3f, 0.3f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.Start))]
    private static void OnProbeLauncherAwake(ProbeLauncher __instance)
    {
        if (!__instance.GetComponentInParent<PlayerBody>()) return;
        Create(__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis"), new Vector3(0.0556f, -0.5962f, 0.0299f), Quaternion.Euler(24.6841f, 0f, 0f), new Vector3(0.9f, 0.9f, 0.9f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.Start))]
    private static void OnTranslatorAwake(NomaiTranslatorProp __instance)
    {
        Create(__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo"), new Vector3(0.6251f, -0.5804f, -0.2715f), Quaternion.identity, new Vector3(1.2f, 1.2f, 1.2f));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SharedStone), nameof(SharedStone.Awake))]
    private static void OnSharedStoneAwake(SharedStone __instance)
    {
        Create(__instance.transform.Find("AnimRoot/Props_NOM_SharedStone"), new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), new Vector3(0.9f, 0.9f, 0.9f), true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScrollItem), nameof(ScrollItem.Awake))]
    private static void OnScrollAwake(SharedStone __instance)
    {
        Vector3 position = __instance.name == "Prefab_NOM_Scroll_egg" ? new Vector3(-0.2028f, 0.0195f, -0.2974f) : new Vector3(-0.1748f, 0.0613f, -0.2957f);
        Create(__instance.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo"), position, Quaternion.Euler(358.7909f, 107.971f, 3.502f), new Vector3(0.9f, 0.9f, 0.9f), true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiConversationStone), nameof(NomaiConversationStone.Awake))]
    private static void OnSolanumStoneAwake(NomaiConversationStone __instance)
    {
        foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.name.Contains("_Back"))
            {
                Create(renderer.transform, new Vector3(0.1748f, -0.1398f, -0.2008f), Quaternion.Euler(0f, 0f, 292.1743f), new Vector3(0.6f, 0.6f, 0.6f), true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WarpCoreItem), nameof(WarpCoreItem.Awake))]
    private static void OnWarpCoreAwake(WarpCoreItem __instance)
    {
        if (__instance._warpCoreType == WarpCoreType.Vessel)
        {
            Create(__instance.transform.Find("Props_NOM_WarpCore_Advanced/Props_NOM_WarpCore_Advance_Geo"), new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), new Vector3(0.9f, 0.9f, 0.9f), true);
        }
        else if (__instance._warpCoreType == WarpCoreType.VesselBroken)
        {
            Create(__instance.transform.Find("Props_NOM_WarpCore_Advanced_Broken_V3/Props_NOM_WarpCore_Advance_Broken_Geo"), new Vector3(-0.2098f, -0.3825f, 0.0593f), Quaternion.Euler(8.5636f, 156.946f, 331.5615f), new Vector3(0.9f, 0.9f, 0.9f), true);
        }
        else
        {
            Create(__instance.transform.Find("Props_NOM_WarpCore_Simple"), new Vector3(0.057f, -0.4437f, -0.4526f), Quaternion.Euler(323.3099f, 77.0467f, 330.0953f), new Vector3(2f, 2f, 2f), true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SimpleLanternItem), nameof(SimpleLanternItem.Start))]
    private static void OnSimpleLanternAwake(SimpleLanternItem __instance)
    {
        ViewmodelArm arm = Create(__instance.transform.Find("Props_IP_Lantern/Lantern_geo"), new Vector3(-0.2524f, 0.2953f, -0.0524f), Quaternion.Euler(330f, 140f, 90f), new Vector3(1.2f, 1.2f, 1.2f), true);
        if (arm == null)
        {
            arm = Create(__instance.transform.Find("Props_IP_Lantern_Crack/Lantern_geo"), new Vector3(0.2494f, 0.6859f, 0.0476f), Quaternion.Euler(330f, 320f, 90f), new Vector3(1.2f, 1.2f, 1.2f), true);
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
                Create(renderer.transform, new Vector3(-0.4143f, 0.1576f, -0.2241f), Quaternion.Euler(4.0031f, 145.1847f, 70.3509f), new Vector3(0.9f, 0.9f, 0.9f), true);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.Start))]
    private static void OnDreamLanternAwake(DreamLanternItem __instance)
    {
        ViewmodelArm arm = Create(__instance.transform.Find("Props_IP_Artifact_ViewModel/artifact_geo"), new Vector3(0.15f, 0.3f, -0.16f), Quaternion.Euler(330, 0, 90), new Vector3(1.2f, 1.2f, 1.2f));
        if (arm == null)
        {
            arm = Create(__instance.transform.Find("ViewModel/Props_IP_DreamLanternItem_Malfunctioning (1)/PrototypeArtifact_2"), new Vector3(0.1389f, 0.836f, -0.144f), Quaternion.Euler(0, 0, 90), new Vector3(1.2f, 1.2f, 1.2f));
            if (arm == null)
            {
                arm = Create(__instance.transform.Find("Props_IP_DreamLanternItem_Nonfunctioning/PrototypeArtifact"), new Vector3(0.1593f, 0.7578f, -0.144f), Quaternion.Euler(330, 0, 90), new Vector3(1.2f, 1.2f, 1.2f));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.Start))]
    private static void OnVisionTorchAwake(VisionTorchItem __instance)
    {
        Create(__instance.transform.Find("Prefab_IP_VisionTorchProjector/Props_IP_ScannerStaff/Scannerstaff_geo"), new Vector3(0.0403f, 1.0224f, 0.141f), Quaternion.Euler(345.0329f, 184.0765f, 358.0521f), Vector3.one, true);
    }
}
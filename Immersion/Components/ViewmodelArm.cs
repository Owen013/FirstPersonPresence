using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    public enum ArmTransform
    {
        Signalscope,
        ProbeLauncher,
        Translator,
        SharedStone,
        Scroll,
        ScrollEasterEgg,
        NomaiConversationStone,
        WarpCore,
        WarpCoreBroken,
        WarpCoreSimple,
        SimpleLantern,
        SimpleLanternCracked,
        SlideReel,
        DreamLantern,
        DreamLanternPrototype,
        DreamLanternPrototype2,
        VisionTorch
    }

    public enum ArmShader
    {
        Default,
        Viewmodel
    }

    public static readonly (Vector3 position, Quaternion rotation, float scale)[] armTransforms =
    {
        (new Vector3(-0.01f, -0.11f, -0.16f), Quaternion.identity, 0.3f), // Signalscope
        (new Vector3(0.0556f, -0.5962f, 0.0299f), Quaternion.Euler(24.6841f, 0f, 0f), 0.9f), // ProbeLauncher
        (new Vector3(0.6251f, -0.5804f, -0.2715f), Quaternion.identity, 1.2f), // Translator
        (new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), 0.9f), // SharedStone
        (new Vector3(-0.1748f, 0.0613f, -0.2957f), Quaternion.Euler(358.7909f, 107.971f, 3.502f), 0.9f), // Scroll
        (new Vector3(-0.2028f, 0.0195f, -0.2974f), Quaternion.Euler(358.7909f, 107.971f, 3.502f), 0.9f), // BrittleHollowScroll
        (new Vector3(0.1748f, -0.1398f, -0.2008f), Quaternion.Euler(0f, 0f, 292.1743f), 0.6f), // NomaiConversationStone
        (new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), 0.9f), // WarpCore
        (new Vector3(-0.2098f, -0.3825f, 0.0593f), Quaternion.Euler(8.5636f, 156.946f, 331.5615f), 0.9f), // WarpCoreBroken
        (new Vector3(0.057f, -0.4437f, -0.4526f), Quaternion.Euler(323.3099f, 77.0467f, 330.0953f), 2f), // WarpCoreSimple
        (new Vector3(-0.2524f, 0.2953f, -0.0524f), Quaternion.Euler(330f, 140f, 90f), 1.2f), // SimpleLantern
        (new Vector3(0.2494f, 0.6859f, 0.0476f), Quaternion.Euler(330f, 320f, 90f), 1.2f), // SimpleLanternCracked
        (new Vector3(-0.4143f, 0.1576f, -0.2241f), Quaternion.Euler(4.0031f, 145.1847f, 70.3509f), 0.9f), // SlideReel
        (new Vector3(0.15f, 0.3f, -0.16f), Quaternion.Euler(330f, 0f, 90f), 1.2f), // DreamLantern
        (new Vector3(0.1593f, 0.7578f, -0.144f), Quaternion.Euler(330f, 0f, 90f), 1.2f), // DreamLanternPrototype
        (new Vector3(0.1389f, 0.836f, -0.144f), Quaternion.Euler(0f, 0f, 90f), 1.2f), // DreamLanternPrototype2
        (new Vector3(0.0403f, 1.0224f, 0.141f), Quaternion.Euler(345.0329f, 184.0765f, 358.0521f), 1) // VisionTorch
    };

    public static Shader DefaultNoSuitShader;

    public static Shader DefaultSuitShader;

    public static Shader ViewmodelShader;

    private GameObject _playerModelUnsuitedRightArm;

    private GameObject _playerModelSuitedRightArm;

    private GameObject _playerModelUnsuitedLeftArm;

    private GameObject _playerModelSuitedLeftArm;

    private GameObject _noSuitModel;

    private GameObject _suitModel;

    private OWItem _owItem;

    public static ViewmodelArm NewViewmodelArm(Transform parent, (Vector3 position, Quaternion rotation, float scale) armTransform, ArmShader shader, OWItem owItem = null, bool replace = false)
    {
        if (parent.Find("ViewmodelArm") is var existingArm && existingArm != null)
        {
            if (replace) GameObject.Destroy(existingArm);
            else return existingArm.GetComponent<ViewmodelArm>();
        }

        var arm = new GameObject("ViewmodelArm").AddComponent<ViewmodelArm>();
        arm.transform.parent = parent;
        arm.transform.localPosition = armTransform.position;
        arm.transform.localRotation = armTransform.rotation;
        arm.transform.localScale = armTransform.scale * Vector3.one;
        arm._owItem = owItem;
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => arm.SetShader(shader));

        if (owItem != null)
        {
            owItem.onPickedUp += (item) =>
            {
                arm.gameObject.SetActive(true);
                arm.enabled = true;
            };
        }

        return arm;
    }

    public static void GetArmShaders()
    {
        DefaultNoSuitShader ??= Resources.FindObjectsOfTypeAll<Shader>().Where(shader => shader.name == "Standard").FirstOrDefault();
        DefaultSuitShader ??= Resources.FindObjectsOfTypeAll<Shader>().Where(shader => shader.name == "Outer Wilds/Environment/Foliage").FirstOrDefault();
        ViewmodelShader ??= Resources.FindObjectsOfTypeAll<Shader>().Where(shader => shader.name == "Outer Wilds/Utility/View Model (Cutoff)").FirstOrDefault();
    }

    public void SetShader(ArmShader shader)
    {
        GetArmShaders();
        MeshRenderer noSuitMesh = _noSuitModel.GetComponent<MeshRenderer>();
        MeshRenderer suitMesh = _suitModel.GetComponent<MeshRenderer>();
        if (shader == ArmShader.Viewmodel)
        {
            noSuitMesh.materials[0].shader = ViewmodelShader;
            noSuitMesh.materials[1].shader = ViewmodelShader;
            suitMesh.material.shader = ViewmodelShader;
        }
        else
        {
            noSuitMesh.materials[0].shader = DefaultNoSuitShader;
            noSuitMesh.materials[1].shader = DefaultNoSuitShader;
            suitMesh.material.shader = DefaultSuitShader;
        }
    }

    private void Awake()
    {
        Transform playerTransform = Locator.GetPlayerController().transform;
        _playerModelUnsuitedRightArm = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerModelSuitedRightArm = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        _playerModelUnsuitedLeftArm = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
        _playerModelSuitedLeftArm = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;

        _noSuitModel = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        _noSuitModel.transform.parent = transform;
        _noSuitModel.layer = 27;
        _noSuitModel.transform.localPosition = Vector3.zero;
        _noSuitModel.transform.localRotation = Quaternion.Euler(330, 0, 300);
        _noSuitModel.transform.localScale = Vector3.one;
        MeshRenderer noSuitMesh = _noSuitModel.GetComponent<MeshRenderer>();
        noSuitMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _noSuitModel.SetActive(false);

        _suitModel = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        _suitModel.transform.parent = transform;
        _suitModel.layer = 27;
        _suitModel.transform.localPosition = new Vector3(-0.02f, 0.03f, 0.02f);
        _suitModel.transform.localRotation = Quaternion.Euler(330, 0, 300);
        _suitModel.transform.localScale = Vector3.one;
        MeshRenderer suitMesh = _suitModel.GetComponent<MeshRenderer>();
        suitMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        suitMesh.material.renderQueue = noSuitMesh.material.renderQueue;
        _suitModel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_owItem != null && Locator.GetToolModeSwapper()?._itemCarryTool._heldItem != _owItem) gameObject.SetActive(false);

        if (!ModMain.Instance.IsViewModelHandsEnabled)
        {
            _noSuitModel.SetActive(false);
            _suitModel.SetActive(false);
        }
        else if (ModMain.Instance.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
        {
            _noSuitModel.SetActive(_playerModelUnsuitedLeftArm.activeInHierarchy);
            _suitModel.SetActive(_playerModelSuitedLeftArm.activeInHierarchy);
        }
        else
        {
            _noSuitModel.SetActive(_playerModelUnsuitedRightArm.activeInHierarchy);
            _suitModel.SetActive(_playerModelSuitedRightArm.activeInHierarchy);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.EquipTool))]
    private static void SignalscopeEquipped(Signalscope __instance)
    {
        if (__instance.transform.Find("Props_HEA_Signalscope/ViewmodelArm") || !__instance.GetComponentInParent<PlayerBody>()) return;
        NewViewmodelArm(__instance.transform.Find("Props_HEA_Signalscope"), armTransforms[(int)ArmTransform.Signalscope], ArmShader.Viewmodel);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.EquipTool))]
    private static void ProbeLauncherEquipped(ProbeLauncher __instance)
    {
        if (__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis/ViewmodelArm") || !__instance.GetComponentInParent<PlayerBody>()) return;
        NewViewmodelArm(__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis"), armTransforms[(int)ArmTransform.ProbeLauncher], ArmShader.Viewmodel);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.OnEquipTool))]
    private static void TranslatorEquipped(NomaiTranslatorProp __instance)
    {
        if (__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo/ViewmodelArm")) return;
        NewViewmodelArm(__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo"), armTransforms[(int)ArmTransform.Translator], ArmShader.Viewmodel);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void ItemPickedUp(OWItem __instance)
    {
        if (__instance.GetComponent<SharedStone>() is var sharedStone && sharedStone != null)
        {
            NewViewmodelArm(sharedStone.transform.Find("AnimRoot/Props_NOM_SharedStone"), armTransforms[(int)ArmTransform.SharedStone], ArmShader.Default, sharedStone);
        }
        else if (__instance.GetComponent<ScrollItem>() is var scroll && scroll != null)
        {
            if (scroll.name == "Prefab_NOM_Scroll_egg")
            {
                NewViewmodelArm(scroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo"), armTransforms[(int)ArmTransform.ScrollEasterEgg], ArmShader.Default, scroll);
            }
            else
            {
                NewViewmodelArm(scroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo"), armTransforms[(int)ArmTransform.Scroll], ArmShader.Default, scroll);
            }
        }
        else if (__instance.GetComponent<NomaiConversationStone>() is var conversationStone && conversationStone != null)
        {
            foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.name.Contains("_Back"))
                {
                    NewViewmodelArm(renderer.transform, armTransforms[(int)ArmTransform.NomaiConversationStone], ArmShader.Default, conversationStone);
                }
            }
        }
        else if (__instance.GetComponent<WarpCoreItem>() is var warpCore && warpCore != null)
        {
            switch (warpCore._warpCoreType)
            {
                case WarpCoreType.Vessel:
                    NewViewmodelArm(__instance.transform.Find("Props_NOM_WarpCore_Advanced/Props_NOM_WarpCore_Advance_Geo"), armTransforms[(int)ArmTransform.WarpCore], ArmShader.Default, warpCore);
                    break;
                case WarpCoreType.VesselBroken:
                    NewViewmodelArm(__instance.transform.Find("Props_NOM_WarpCore_Advanced_Broken_V3/Props_NOM_WarpCore_Advance_Broken_Geo"), armTransforms[(int)ArmTransform.WarpCoreBroken], ArmShader.Default, warpCore);
                    break;
                default:
                    NewViewmodelArm(__instance.transform.Find("Props_NOM_WarpCore_Simple"), armTransforms[(int)ArmTransform.WarpCoreSimple], ArmShader.Default, warpCore);
                    break;
            }
        }
        else if (__instance.GetComponent<SimpleLanternItem>() is var simpleLantern && simpleLantern != null)
        {
            Transform transform = __instance.transform.Find("Props_IP_Lantern/Lantern_geo");
            if (transform == null)
            {
                NewViewmodelArm(__instance.transform.Find("Props_IP_Lantern_Crack/Lantern_geo"), armTransforms[(int)ArmTransform.SimpleLanternCracked], ArmShader.Default, simpleLantern);
            }
            else
            {
                NewViewmodelArm(transform, armTransforms[(int)ArmTransform.SimpleLantern], ArmShader.Default, simpleLantern);
            }
        }
        else if (__instance.GetComponent<SlideReelItem>() is var slideReel && slideReel != null)
        {
            foreach (MeshRenderer renderer in __instance.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.name.Contains("Frame_"))
                {
                    NewViewmodelArm(renderer.transform, armTransforms[(int)ArmTransform.SlideReel], ArmShader.Default, slideReel);
                }
            }
        }
        else if (__instance.GetComponent<DreamLanternItem>() is var dreamLantern && dreamLantern != null)
        {
            Transform transform = __instance.transform.Find("Props_IP_Artifact_ViewModel/artifact_geo");
            if (transform == null)
            {
                transform = __instance.transform.Find("ViewModel/Props_IP_DreamLanternItem_Malfunctioning (1)/PrototypeArtifact_2");
                if (transform == null)
                {
                    NewViewmodelArm(__instance.transform.Find("Props_IP_DreamLanternItem_Nonfunctioning/PrototypeArtifact"), armTransforms[(int)ArmTransform.DreamLanternPrototype], ArmShader.Viewmodel, dreamLantern);
                }
                else
                {
                    NewViewmodelArm(transform, armTransforms[(int)ArmTransform.DreamLanternPrototype2], ArmShader.Default, dreamLantern);
                }
            }
            else
            {
                NewViewmodelArm(transform, armTransforms[(int)ArmTransform.DreamLantern], ArmShader.Viewmodel, dreamLantern);
            }
        }
        else if (__instance.GetComponent<VisionTorchItem>() is var visionTorch && visionTorch != null)
        {
            NewViewmodelArm(__instance.transform.Find("Prefab_IP_VisionTorchProjector/Props_IP_ScannerStaff/Scannerstaff_geo"), armTransforms[(int)ArmTransform.VisionTorch], ArmShader.Default, visionTorch);
        }
    }
}
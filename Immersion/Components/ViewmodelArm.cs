using HarmonyLib;
using OWML.Common;
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
        NomaiConversationStone,
        WarpCore,
        WarpCoreBroken,
        WarpCoreSimple,
        SimpleLantern,
        SlideReel,
        DreamLantern,
        DreamLanternNonfunctioning,
        VisionTorch
    }

    public enum ArmShader
    {
        Standard,
        Viewmodel,
        ViewmodelCutoff
    }

    private static readonly (Vector3 position, Quaternion rotation, float scale)[] s_armTransforms =
    {
        (new Vector3(-0.01f, -0.11f, -0.16f), Quaternion.identity, 0.3f), // Signalscope
        (new Vector3(0.0556f, -0.5962f, 0.0299f), Quaternion.Euler(24.6841f, 0f, 0f), 0.9f), // ProbeLauncher
        (new Vector3(0.6251f, -0.5804f, -0.2715f), Quaternion.identity, 1.2f), // Translator
        (new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), 0.9f), // SharedStone
        (new Vector3(-0.1748f, 0.0613f, -0.6657f), Quaternion.Euler(358.7909f, 107.971f, 3.502f), 0.9f), // Scroll
        (new Vector3(0.1748f, -0.1898f, -0.2008f), Quaternion.Euler(0f, 0f, 292.1743f), 0.6f), // NomaiConversationStone
        (new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), 0.9f), // WarpCore
        (new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), 0.9f), // WarpCoreBroken
        (new Vector3(0.0285f, -0.1719f, -0.2263f), Quaternion.Euler(323.3099f, 77.0467f, 330.0953f), 1), // WarpCoreSimple
        (new Vector3(0.256f, 0.6861f, 0.0302f), Quaternion.Euler(330f, 325f, 90f), 1.2f), // SimpleLantern
        (new Vector3(-0.4219f, 0.3641f, -0.2282f), Quaternion.Euler(4.0031f, 145.1847f, 70.3509f), 1), // SlideReel
        (new Vector3(0.3205f, 0.6353f, -0.1311f), Quaternion.Euler(330.5013f, 20.7251f, 78.4916f), 1.2f), // DreamLantern
        (new Vector3(0.1593f, 0.7578f, -0.144f), Quaternion.Euler(330f, 0f, 90f), 1.2f), // DreamLanternPrototype
        (new Vector3(-0.0403f, 0.0674f, -0.141f), Quaternion.Euler(345.0329f, 4.0765f, 358.0521f), 1) // VisionTorch
    };

    private static Shader[] s_armShaders;

    private GameObject _playerModelRightArmNoSuit;

    private GameObject _playerModelRightArmSuit;

    private GameObject _playerModelLeftArmNoSuit;

    private GameObject _playerModelLeftArmSuit;

    private GameObject _viewmodelArmNoSuit;

    private GameObject _viewmodelArmSuit;

    private OWItem _owItem;

    public static ViewmodelArm NewViewmodelArm(Transform armParent, (Vector3 position, Quaternion rotation, float scale) armTransform, ArmShader armShader, OWItem owItem = null)
    {
        // replace viewmodel arm if it already exists
        if (armParent.Find("ViewmodelArm") is var existingArm && existingArm != null)
        {
            ModMain.Instance.Log(armParent.name + " already has a viewmodel arm. Replacing it", MessageType.Warning);
            GameObject.Destroy(existingArm.gameObject);
        }

        var viewmodelArm = new GameObject("ViewmodelArm").AddComponent<ViewmodelArm>();
        viewmodelArm.transform.parent = armParent;
        viewmodelArm.transform.localPosition = armTransform.position;
        viewmodelArm.transform.localRotation = armTransform.rotation;
        viewmodelArm.transform.localScale = armTransform.scale * Vector3.one;
        viewmodelArm._owItem = owItem;
        viewmodelArm.SetArmShader(armShader);

        if (owItem != null)
        {
            owItem.onPickedUp += (item) =>
            {
                viewmodelArm.gameObject.SetActive(true);
            };
        }

        return viewmodelArm;
    }

    public static ViewmodelArm NewViewmodelArm(Transform armParent, ArmTransform armTransform, ArmShader armShader, OWItem owItem = null)
    {
        return NewViewmodelArm(armParent, s_armTransforms[(int)armTransform], armShader, owItem);
    }

    public void SetArmShader(ArmShader armShader)
    {
        // get shaders if we don't have them
        s_armShaders ??=
        [
            Shader.Find("Standard"),
            Shader.Find("Outer Wilds/Utility/View Model"),
            Shader.Find("Outer Wilds/Utility/View Model (Cutoff)")
        ];

        MeshRenderer noSuitMesh = _viewmodelArmNoSuit.GetComponent<MeshRenderer>();
        noSuitMesh.materials[0].shader = s_armShaders[(int)armShader];
        noSuitMesh.materials[1].shader = s_armShaders[(int)armShader];
        _viewmodelArmSuit.GetComponent<MeshRenderer>().material.shader = s_armShaders[(int)armShader];
    }

    private void Awake()
    {
        // grab references to the player's real arms
        Transform playerTransform = Locator.GetPlayerController().transform;
        _playerModelRightArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerModelRightArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        _playerModelLeftArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
        _playerModelLeftArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;

        // copy nosuit arm from marshmallow stick
        _viewmodelArmNoSuit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        _viewmodelArmNoSuit.transform.parent = transform;
        _viewmodelArmNoSuit.layer = 27;
        _viewmodelArmNoSuit.transform.localPosition = Vector3.zero;
        _viewmodelArmNoSuit.transform.localRotation = Quaternion.Euler(330, 0, 300);
        _viewmodelArmNoSuit.transform.localScale = Vector3.one;
        MeshRenderer noSuitMesh = _viewmodelArmNoSuit.GetComponent<MeshRenderer>();
        noSuitMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _viewmodelArmNoSuit.SetActive(false);

        // copy suit arm from marshmallow stick
        _viewmodelArmSuit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        _viewmodelArmSuit.transform.parent = transform;
        _viewmodelArmSuit.layer = 27;
        _viewmodelArmSuit.transform.localPosition = new Vector3(-0.02f, 0.03f, 0.02f);
        _viewmodelArmSuit.transform.localRotation = Quaternion.Euler(330, 0, 300);
        _viewmodelArmSuit.transform.localScale = Vector3.one;
        MeshRenderer suitMesh = _viewmodelArmSuit.GetComponent<MeshRenderer>();
        suitMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        suitMesh.material.renderQueue = noSuitMesh.material.renderQueue;
        _viewmodelArmSuit.SetActive(false);
    }

    private void LateUpdate()
    {
        // if this is an item and it isn't being held, disable the arm gameobject
        if (_owItem != null && Locator.GetToolModeSwapper()?._itemCarryTool._heldItem != _owItem)
        {
            gameObject.SetActive(false);
            return;
        }

        // disable viewmodel arm if disabled in config
        if (!ModMain.Instance.IsViewModelHandsEnabled)
        {
            _viewmodelArmNoSuit.SetActive(false);
            _viewmodelArmSuit.SetActive(false);
        }
        // if lefty mode, use the clothing of the left arm for the viewmodel arm
        else if (ModMain.Instance.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
        {
            _viewmodelArmNoSuit.SetActive(_playerModelLeftArmNoSuit.activeInHierarchy);
            _viewmodelArmSuit.SetActive(_playerModelLeftArmSuit.activeInHierarchy);
        }
        // otherwise, use right arm clothing (default)
        else
        {
            _viewmodelArmNoSuit.SetActive(_playerModelRightArmNoSuit.activeInHierarchy);
            _viewmodelArmSuit.SetActive(_playerModelRightArmSuit.activeInHierarchy);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.EquipTool))]
    private static void SignalscopeEquipped(Signalscope __instance)
    {
        // don't create viewmodel arm if 1. it's disabled in config, 2. if there already one there, or 3. it's not a child of the player
        if (!ModMain.Instance.IsViewModelHandsEnabled || __instance.transform.Find("Props_HEA_Signalscope/ViewmodelArm") || !__instance.GetComponentInParent<PlayerBody>()) return;
        NewViewmodelArm(__instance.transform.Find("Props_HEA_Signalscope"), s_armTransforms[(int)ArmTransform.Signalscope], ArmShader.ViewmodelCutoff);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.EquipTool))]
    private static void ProbeLauncherEquipped(ProbeLauncher __instance)
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled || __instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis/ViewmodelArm") || !__instance.GetComponentInParent<PlayerBody>()) return;
        NewViewmodelArm(__instance.transform.Find("Props_HEA_ProbeLauncher/ProbeLauncherChassis"), s_armTransforms[(int)ArmTransform.ProbeLauncher], ArmShader.ViewmodelCutoff);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.OnEquipTool))]
    private static void TranslatorEquipped(NomaiTranslatorProp __instance)
    {
        // only the player has a translator, so don't need to check for that
        if (!ModMain.Instance.IsViewModelHandsEnabled || __instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo/ViewmodelArm")) return;
        NewViewmodelArm(__instance.transform.Find("TranslatorGroup/Props_HEA_Translator/Props_HEA_Translator_Geo"), s_armTransforms[(int)ArmTransform.Translator], ArmShader.ViewmodelCutoff);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void ItemPickedUp(OWItem __instance)
    {
        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (!ModMain.Instance.IsViewModelHandsEnabled || __instance.transform.Find("ViewmodelArm")) return;

        switch (__instance._type)
        {
            case ItemType.SharedStone:
                NewViewmodelArm(__instance.transform, ArmTransform.SharedStone, ArmShader.Standard, __instance);
                break;
            case ItemType.Scroll:
                if (__instance.name == "Prefab_NOM_Scroll_Jeff")
                {
                    NewViewmodelArm(__instance.transform, (new Vector3(0.2107f, - 0.0169f, 0.167f), Quaternion.Euler(358.7909f, 287.9709f, 59.1747f), 0.9f), ArmShader.Standard, __instance);
                }
                else
                {
                    NewViewmodelArm(__instance.transform, ArmTransform.Scroll, ArmShader.Standard, __instance);
                }
                break;
            case ItemType.ConversationStone:
                NewViewmodelArm(__instance.transform, ArmTransform.NomaiConversationStone, ArmShader.Standard, __instance);
                break;
            case ItemType.WarpCore:
                switch ((__instance as WarpCoreItem)._warpCoreType)
                {
                    case WarpCoreType.Vessel:
                        NewViewmodelArm(__instance.transform, ArmTransform.WarpCore, ArmShader.Standard, __instance);
                        break;
                    case WarpCoreType.VesselBroken:
                        NewViewmodelArm(__instance.transform, ArmTransform.WarpCoreBroken, ArmShader.Standard, __instance);
                        break;
                    default:
                        NewViewmodelArm(__instance.transform, ArmTransform.WarpCoreSimple, ArmShader.Standard, __instance);
                        break;
                }
                break;
            case ItemType.Lantern:
                NewViewmodelArm(__instance.transform, ArmTransform.SimpleLantern, ArmShader.Standard, __instance);
                break;
            case ItemType.SlideReel:
                NewViewmodelArm(__instance.transform, ArmTransform.SlideReel, ArmShader.Standard, __instance);
                break;
            case ItemType.DreamLantern:
                if ((__instance as DreamLanternItem)._lanternType == DreamLanternType.Nonfunctioning)
                {
                    NewViewmodelArm(__instance.transform, ArmTransform.DreamLanternNonfunctioning, ArmShader.Standard, __instance);
                }
                else
                {
                    NewViewmodelArm(__instance.transform, ArmTransform.DreamLantern, ArmShader.Viewmodel, __instance);
                }
                break;
            case ItemType.VisionTorch:
                NewViewmodelArm(__instance.transform, ArmTransform.VisionTorch, ArmShader.Standard, __instance);
                break;
        }
    }
}
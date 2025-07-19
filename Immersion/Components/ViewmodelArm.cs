using HarmonyLib;
using OWML.Common;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    public enum ArmShader
    {
        Standard,
        Viewmodel,
        ViewmodelCutoff
    }

    private static GameObject s_playerRightArmNoSuit;

    private static GameObject s_playerRightArmSuit;

    private static GameObject s_playerLeftArmNoSuit;

    private static GameObject s_playerLeftArmSuit;

    private static bool s_isPlayerArmRefsInitialized;

    private static Shader[] s_armShaders;

    private GameObject _viewmodelArmNoSuit;

    private GameObject _viewmodelArmSuit;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool, (Vector3 position, Quaternion rotation, float scale) armTransform, ArmShader armShader = ArmShader.Standard)
    {
        return NewViewmodelArm(armTransform, playerTool, null, armShader);
    }

    public static ViewmodelArm NewViewmodelArm(OWItem owItem, (Vector3 position, Quaternion rotation, float scale) armTransform, ArmShader armShader = ArmShader.Standard)
    {
        return NewViewmodelArm(armTransform, null, owItem, armShader);
    }

    private static ViewmodelArm NewViewmodelArm((Vector3 position, Quaternion rotation, float scale) armTransform, PlayerTool playerTool = null, OWItem owItem = null, ArmShader armShader = ArmShader.Standard)
    {
        var armParent = playerTool?.transform ?? owItem?.transform;

        // replace viewmodel arm if it already exists
        var existingArm = armParent.Find("ViewmodelArm");
        if (existingArm != null)
        {
            ModMain.Instance.ModHelper.Console.WriteLine($"{armParent.name} already has a viewmodel arm. Replacing it", MessageType.Warning);
            GameObject.Destroy(existingArm.gameObject);
        }

        var viewmodelArm = new GameObject("ViewmodelArm").AddComponent<ViewmodelArm>();
        viewmodelArm.transform.parent = armParent;
        viewmodelArm.transform.localPosition = armTransform.position;
        viewmodelArm.transform.localRotation = armTransform.rotation;
        viewmodelArm.transform.localScale = armTransform.scale * Vector3.one;
        viewmodelArm.SetArmShader(armShader);

        if (playerTool != null)
        {
            viewmodelArm._playerTool = playerTool;
        }
        else
        {
            viewmodelArm._owItem = owItem;
            owItem.onPickedUp += (item) =>
            {
                viewmodelArm.gameObject.SetActive(true);
            };
        }

        return viewmodelArm;
    }

    private void SetArmShader(ArmShader armShader)
    {
        // get shaders if we don't have them
        s_armShaders ??=
        [
            Shader.Find("Standard"),
            Shader.Find("Outer Wilds/Utility/View Model"),
            Shader.Find("Outer Wilds/Utility/View Model (Cutoff)")
        ];

        // apply shaders to materials
        MeshRenderer noSuitMesh = _viewmodelArmNoSuit.GetComponent<MeshRenderer>();
        noSuitMesh.materials[0].shader = s_armShaders[(int)armShader];
        noSuitMesh.materials[1].shader = s_armShaders[(int)armShader];
        _viewmodelArmSuit.GetComponent<MeshRenderer>().material.shader = s_armShaders[(int)armShader];
    }

    private void Awake()
    {
        if (!s_isPlayerArmRefsInitialized)
        {
            // grab references to the player's real arms
            Transform playerTransform = Locator.GetPlayerController().transform;
            s_playerRightArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
            s_playerRightArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
            s_playerLeftArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
            s_playerLeftArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;

            s_isPlayerArmRefsInitialized = true;
        }

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
        suitMesh.material.renderQueue = noSuitMesh.material.renderQueue; // what does this do?
        _viewmodelArmSuit.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_playerTool != null)
        {
            if (!_playerTool._isEquipped && !_playerTool._isPuttingAway)
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else if (_owItem != null && Locator.GetToolModeSwapper()._itemCarryTool._heldItem != _owItem)
        {
            gameObject.SetActive(false);
            return;
        }

        // if lefty mode, use the clothing of the left arm for the viewmodel arm UNLESS using translator, which is held with right hand no matter what
        if (ModMain.Instance.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
        {
            _viewmodelArmNoSuit.SetActive(s_playerLeftArmNoSuit.activeInHierarchy);
            _viewmodelArmSuit.SetActive(s_playerLeftArmSuit.activeInHierarchy);
        }
        // otherwise, use right arm clothing (default)
        else
        {
            _viewmodelArmNoSuit.SetActive(s_playerRightArmNoSuit.activeInHierarchy);
            _viewmodelArmSuit.SetActive(s_playerRightArmSuit.activeInHierarchy);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.EquipTool))]
    private static void SignalscopeEquipped(Signalscope __instance)
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled) return;

        if (__instance.transform.Find("ViewmodelArm"))
        {
            __instance.transform.Find("ViewmodelArm").gameObject.SetActive(true);
            return;
        }

        NewViewmodelArm(__instance, (new Vector3(0.2183f, -0.2501f, 0.2651f), Quaternion.Euler(0f, 0.8f, 0.3f), 0.3f), ArmShader.ViewmodelCutoff);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.EquipTool))]
    private static void ProbeLauncherEquipped(ProbeLauncher __instance)
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled) return;

        if (__instance.transform.Find("ViewmodelArm"))
        {
            __instance.transform.Find("ViewmodelArm").gameObject.SetActive(true);
            return;
        }

        NewViewmodelArm(__instance, (new Vector3(0.3482f, -0.9607f, 0.9992f), Quaternion.Euler(24.6841f, 0f, 0f), 0.9f), ArmShader.ViewmodelCutoff);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NomaiTranslator), nameof(NomaiTranslator.EquipTool))]
    private static void TranslatorEquipped(NomaiTranslator __instance)
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled) return;

        if (__instance.transform.Find("ViewmodelArm"))
        {
            __instance.transform.Find("ViewmodelArm").gameObject.SetActive(true);
            return;
        }

        NewViewmodelArm(__instance, (new Vector3(0.7441f, -0.9404f, 0.7593f), Quaternion.Euler(0f, 3.5522f, 0f), 1.2f), ArmShader.ViewmodelCutoff);
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
                NewViewmodelArm(__instance, (new Vector3(0.1865f, -0.0744f, -0.2171f), Quaternion.Euler(0f, 320f, 310f), 0.9f));
                break;
            case ItemType.Scroll:
                if (__instance.name == "Prefab_NOM_Scroll_Jeff")
                {
                    NewViewmodelArm(__instance, (new Vector3(0.2107f, -0.0169f, 0.167f), Quaternion.Euler(358.7909f, 287.9709f, 59.1747f), 0.9f));
                }
                else
                {
                    NewViewmodelArm(__instance, (new Vector3(-0.1748f, 0.0613f, -0.6657f), Quaternion.Euler(358.7909f, 107.971f, 3.502f), 0.9f));
                }
                break;
            case ItemType.ConversationStone:
                NewViewmodelArm(__instance, (new Vector3(0.1748f, -0.1898f, -0.2008f), Quaternion.Euler(0f, 0f, 292.1743f), 0.6f));
                break;
            case ItemType.WarpCore:
                switch ((__instance as WarpCoreItem)._warpCoreType)
                {
                    case WarpCoreType.Vessel:
                        NewViewmodelArm(__instance, (new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), 0.9f));
                        break;
                    case WarpCoreType.VesselBroken:
                        NewViewmodelArm(__instance, (new Vector3(0.2098f, -0.3825f, -0.0593f), Quaternion.Euler(8.5636f, 336.946f, 331.5615f), 0.9f));
                        break;
                    default:
                        NewViewmodelArm(__instance, (new Vector3(0.0285f, -0.1719f, -0.2263f), Quaternion.Euler(323.3099f, 77.0467f, 330.0953f), 1f));
                        break;
                }
                break;
            case ItemType.Lantern:
                NewViewmodelArm(__instance, (new Vector3(0.256f, 0.6861f, 0.0302f), Quaternion.Euler(330f, 325f, 90f), 1.2f));
                break;
            case ItemType.SlideReel:
                NewViewmodelArm(__instance, (new Vector3(-0.4219f, 0.3641f, -0.2282f), Quaternion.Euler(4.0031f, 145.1847f, 70.3509f), 1f));
                break;
            case ItemType.DreamLantern:
                if ((__instance as DreamLanternItem)._lanternType == DreamLanternType.Nonfunctioning)
                {
                    NewViewmodelArm(__instance, (new Vector3(0.1593f, 0.7578f, -0.144f), Quaternion.Euler(330f, 0f, 90f), 1.2f));
                }
                else
                {
                    NewViewmodelArm(__instance, (new Vector3(0.3205f, 0.6353f, -0.1311f), Quaternion.Euler(330.5013f, 20.7251f, 78.4916f), 1.2f), ArmShader.Viewmodel);
                }
                break;
            case ItemType.VisionTorch:
                NewViewmodelArm(__instance, (new Vector3(-0.0403f, -0.1344f, -0.125f), Quaternion.Euler(345.0329f, 4.0765f, 358.0521f), 1f));
                break;
        }
    }
}
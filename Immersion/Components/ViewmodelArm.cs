using HarmonyLib;
using Immersion.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    private static GameObject s_viewmodelArmAsset;

    [SerializeField]
    private SkinnedMeshRenderer _armMeshNoSuit;

    [SerializeField]
    private SkinnedMeshRenderer _armMeshSuit;

    [SerializeField]
    private SkinnedMeshRenderer _prePassNoSuit;

    [SerializeField]
    private SkinnedMeshRenderer _prePassSuit;

    private Dictionary<string, Transform> _bones;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    private ItemTool _itemCarryTool;

    private GameObject _playerModelArmNoSuit;

    private GameObject _playerModelArmSuit;

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool)
    {
        var viewmodelArm = NewViewmodelArm(playerTool.transform);
        viewmodelArm._playerTool = playerTool;
        return viewmodelArm;
    }

    public static ViewmodelArm NewViewmodelArm(OWItem owItem)
    {
        var viewmodelArm = NewViewmodelArm(owItem.transform);
        viewmodelArm._owItem = owItem;
        owItem.onPickedUp += (item) => viewmodelArm.gameObject.SetActive(true);
        return viewmodelArm;
    }

    public void SetArmData(string itemName)
    {
        var armData = ArmData.GetArmData(itemName);
        if (armData == null) return;

        transform.localPosition = armData.arm_offset_pos;
        transform.localEulerAngles = armData.arm_offset_rot;
        transform.localScale = 0.1f * Vector3.one * armData.arm_scale;
        SetShader(armData.arm_shader);

        SetBoneEulers(armData.bone_eulers);
    }

    public void OutputArmData()
    {
        string output = "  [ARMDATA NAME HERE] {\n\n";

        var armPos = transform.localPosition;
        output += "    \"arm_offset_pos\": { " + $"\"x\": {armPos.x}, \"y\":  {armPos.y}, \"z\": {armPos.z}" + " },\n";
        var armRot = transform.localEulerAngles;
        output += "    \"arm_offset_rot\": { " + $"\"x\": {armRot.x}, \"y\":  {armRot.y}, \"z\": {armRot.z}" + " },\n";
        output += $"    \"arm_scale\": {10f * transform.localScale.x},\n";
        output += $"    \"arm_shader\": \"{_armMeshNoSuit.material.shader.name}\",\n\n";
        output += "    \"bone_eulers\": {\n";

        foreach (var keyValuePair in _bones)
        {
            var eulers = keyValuePair.Value.localEulerAngles;
            output += $"      \"{keyValuePair.Key}\": " + "{ " + $"\"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z}" + " },\n";
        }

        ModMain.Log(output + "    }\n\n  }");
    }

    internal static void LoadAssetBundleIfNull()
    {
        if (s_viewmodelArmAsset == null)
            s_viewmodelArmAsset = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm").LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        LoadAssetBundleIfNull();

        var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
        viewmodelArm.name = "ViewmodelArm";
        viewmodelArm.transform.parent = parent;
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;

        return viewmodelArm;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance)
    {
        // don't try to add viewmodel arm if disabled in config or if this tool already has one
        if (!Config.EnableViewmodelHands) return;

        // check for existing arm and enable if found (PlayerTool has no event for tool being equipped, so this is required)
        var existingArm = __instance.transform.Find("ViewmodelArm");
        if (existingArm != null)
        {
            existingArm.gameObject.SetActive(true);
            return;
        }

        if (__instance is Signalscope)
        {
            NewViewmodelArm(__instance)?.SetArmData("Signalscope");
            return;
        }

        if (__instance is NomaiTranslator)
        {
            NewViewmodelArm(__instance)?.SetArmData("NomaiTranslator");
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void OWItem_PickUpItem_Postfix(OWItem __instance)
    {
        if (!Config.EnableViewmodelHands) return;

        // rotate lantern to put it in better position for viewmodel arm
        if (__instance.GetItemType() == ItemType.Lantern)
            __instance.transform.localEulerAngles = new Vector3(0f, 327f, 0f);

        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (__instance.transform.Find("ViewmodelArm")) return;

        switch (__instance.GetItemType())
        {
            case ItemType.SharedStone:
                if (__instance is SharedStone)
                    NewViewmodelArm(__instance)?.SetArmData("SharedStone");
                break;

            case ItemType.Scroll:
                if (__instance is ScrollItem)
                {
                    switch(__instance.name)
                    {
                        case "Prefab_NOM_Scroll_egg":
                            NewViewmodelArm(__instance)?.SetArmData("Scroll_Egg");
                            break;

                        case "Prefab_NOM_Scroll_Jeff":
                            NewViewmodelArm(__instance)?.SetArmData("Scroll_Jeff");
                            break;

                        default:
                            NewViewmodelArm(__instance)?.SetArmData("Scroll");
                            break;
                    }
                }
                break;

            case ItemType.ConversationStone:
                if (__instance is NomaiConversationStone solanumStone)
                {
                    if (solanumStone._word == NomaiWord.Identify || solanumStone._word == NomaiWord.Explain)
                        NewViewmodelArm(__instance)?.SetArmData("ConversationStone_Big");
                    else
                        NewViewmodelArm(__instance)?.SetArmData("ConversationStone");
                }
                break;

            case ItemType.WarpCore:
                if (__instance is WarpCoreItem warpCore)
                {
                    if (warpCore._warpCoreType == WarpCoreType.Vessel || warpCore._warpCoreType == WarpCoreType.VesselBroken)
                        NewViewmodelArm(__instance)?.SetArmData("WarpCore");
                    else
                        NewViewmodelArm(__instance)?.SetArmData("WarpCore_Simple");
                }
                break;

            case ItemType.Lantern:
                if (__instance is SimpleLanternItem)
                    NewViewmodelArm(__instance)?.SetArmData("Lantern");
                break;

            case ItemType.SlideReel:
                if (__instance is SlideReelItem)
                    NewViewmodelArm(__instance)?.SetArmData("SlideReel");
                break;

            case ItemType.DreamLantern:
                if (__instance is DreamLanternItem dreamLantern)
                {
                    switch (dreamLantern._lanternType)
                    {
                        case DreamLanternType.Nonfunctioning:
                            NewViewmodelArm(__instance)?.SetArmData("DreamLantern_Nonfunctioning");
                            break;

                        case DreamLanternType.Malfunctioning:
                            NewViewmodelArm(__instance)?.SetArmData("DreamLantern_Malfunctioning");
                            break;

                        default:
                            NewViewmodelArm(__instance)?.SetArmData("DreamLantern");
                            break;
                    }
                }
                break;

            case ItemType.VisionTorch:
                if (__instance is VisionTorchItem)
                    NewViewmodelArm(__instance)?.SetArmData("VisionTorch");
                break;
        }
    }

    private void SetShader(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        _armMeshNoSuit.materials[0].shader = shader;
        _armMeshNoSuit.materials[1].shader = shader;
        _armMeshSuit.material.shader = shader;

        bool isViewmodel = shaderName == "Outer Wilds/Utility/View Model" || shaderName == "Outer Wilds/Utility/View Model (Cutoff)";
        _prePassNoSuit.gameObject.SetActive(isViewmodel);
        _prePassSuit.gameObject.SetActive(isViewmodel);
    }

    private void SetBoneEulers(Dictionary<string, Vector3> boneEulersDict)
    {
        foreach (var boneEulers in boneEulersDict)
            _bones[boneEulers.Key].localEulerAngles = boneEulers.Value;
    }

    private void Awake()
    {
        _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();

        var player = Locator.GetPlayerController().transform;
        _playerModelArmNoSuit = player.transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerModelArmSuit = player.transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;

        _bones = new Dictionary<string, Transform>
        {
            ["Shoulder"] = _armMeshNoSuit.bones[5],
            ["Elbow"] = _armMeshNoSuit.bones[6],
            ["Wrist"] = _armMeshNoSuit.bones[7],
            ["Finger_01_01"] = _armMeshNoSuit.bones[8],
            ["Finger_01_02"] = _armMeshNoSuit.bones[9],
            ["Finger_01_03"] = _armMeshNoSuit.bones[10],
            ["Finger_01_04"] = _armMeshNoSuit.bones[11],
            ["Finger_02_01"] = _armMeshNoSuit.bones[12],
            ["Finger_02_02"] = _armMeshNoSuit.bones[13],
            ["Finger_02_03"] = _armMeshNoSuit.bones[14],
            ["Finger_02_04"] = _armMeshNoSuit.bones[15],
            ["Thumb_01"] = _armMeshNoSuit.bones[16],
            ["Thumb_02"] = _armMeshNoSuit.bones[17],
            ["Thumb_03"] = _armMeshNoSuit.bones[18],
            ["Thumb_04"] = _armMeshNoSuit.bones[19]
        };

        var prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
        _prePassNoSuit.materials[0].shader = prepassShader;
        _prePassNoSuit.materials[1].shader = prepassShader;
        _prePassSuit.material.shader = prepassShader;
    }

    private void LateUpdate()
    {
        if (!Config.EnableViewmodelHands)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_playerTool != null)
        {
            bool isHoldingTool = _playerTool.IsEquipped() || _playerTool.IsPuttingAway();
            if (!isHoldingTool || OWInput.IsInputMode(InputMode.ShipCockpit))
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else if (_owItem != null && _itemCarryTool._heldItem != _owItem)
        {
            gameObject.SetActive(false);
            return;
        }

        _armMeshNoSuit.gameObject.SetActive(_playerModelArmNoSuit.activeInHierarchy);
        _armMeshSuit.gameObject.SetActive(_playerModelArmSuit.activeInHierarchy);
    }
}
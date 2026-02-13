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
        string output = "  [ARMDATA NAME HERE] {\n";
        var pos = transform.localPosition;
        output += "    \"arm_offset_pos\": { " + $"\"x\": {pos.x}, \"y\":  {pos.y}, \"z\": {pos.z}" + " },\n";
        var rot = transform.localEulerAngles;
        output += "    \"arm_offset_rot\": { " + $"\"x\": {rot.x}, \"y\":  {rot.y}, \"z\": {rot.z}" + " },\n";
        output += $"    \"arm_scale\": {10f * transform.localScale.x},\n";
        output += $"    \"arm_shader\": \"{_armMeshNoSuit.material.shader.name}\",\n";
        output += "    \"bone_eulers\": {\n";
        foreach (var keyValuePair in _bones)
        {
            var eulers = keyValuePair.Value.localEulerAngles;
            output += $"      \"{keyValuePair.Key}\": " + "{ " + $"\"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z}" + " },\n";
        }

        ModMain.Log(output + "    }\n  }");
    }

    internal static void LoadAssetBundle()
    {
        if (s_viewmodelArmAsset == null)
            s_viewmodelArmAsset = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm").LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        LoadAssetBundle();
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
        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (!Config.EnableViewmodelHands || __instance.transform.Find("ViewmodelArm")) return;

        switch (__instance._type)
        {
            case ItemType.SharedStone:
                if (__instance is not SharedStone) break;
                NewViewmodelArm(__instance)?.SetArmData("SharedStone");
                break;

            case ItemType.Scroll:
                if (__instance is not ScrollItem) break;

                if (__instance.name == "Prefab_NOM_Scroll_Jeff")
                {
                    NewViewmodelArm(__instance)?.SetArmData("Scroll_Jeff");
                }
                else
                {
                    NewViewmodelArm(__instance)?.SetArmData("Scroll");
                }

                break;

            case ItemType.ConversationStone:
                if (__instance is not NomaiConversationStone) break;
                var word = (__instance as NomaiConversationStone)._word;
                if (word == NomaiWord.Identify || word == NomaiWord.Explain)
                    NewViewmodelArm(__instance)?.SetArmData("ConversationStone_Big");
                else
                    NewViewmodelArm(__instance)?.SetArmData("ConversationStone");
                break;

            case ItemType.WarpCore:
                if (__instance is not WarpCoreItem) break;
                var warpCoreType = (__instance as WarpCoreItem)._warpCoreType;
                if (warpCoreType == WarpCoreType.Vessel || warpCoreType == WarpCoreType.VesselBroken)
                    NewViewmodelArm(__instance)?.SetArmData("WarpCore");
                else
                    NewViewmodelArm(__instance)?.SetArmData("WarpCore_Simple");
                break;

            case ItemType.Lantern:
                if (__instance is not SimpleLanternItem) break;
                NewViewmodelArm(__instance)?.SetArmData("Lantern");
                break;

            case ItemType.SlideReel:
                if (__instance is not SlideReelItem) break;
                NewViewmodelArm(__instance)?.SetArmData("SlideReel");
                break;

            case ItemType.DreamLantern:
                if (__instance is not DreamLanternItem) break;
                switch ((__instance as DreamLanternItem)._lanternType)
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

                break;

            case ItemType.VisionTorch:
                if (__instance is not VisionTorchItem) break;
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
    }

    private void SetBoneEulers(Dictionary<string, Vector3> boneEulersDict)
    {
        foreach (var boneEulers in boneEulersDict)
        {
            _bones[boneEulers.Key].localEulerAngles = boneEulers.Value;
        }
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
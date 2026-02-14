using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

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

        transform.localPosition = armData.armOffsetPos;
        transform.localEulerAngles = armData.armOffsetRot;
        transform.localScale = 0.1f * Vector3.one * armData.armScale;
        SetShader(armData.armShader);

        SetBoneEulers(armData.boneEulers);
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

    internal static void OnEquipTool(PlayerTool tool)
    {
        // don't try to add viewmodel arm if disabled in config or if this tool already has one
        if (!Config.EnableViewmodelArms) return;

        // check for existing arm and enable if found (PlayerTool has no event for tool being equipped, so this is required)
        var existingArm = tool.transform.Find("ViewmodelArm");
        if (existingArm != null)
        {
            existingArm.gameObject.SetActive(true);
            return;
        }

        if (tool is Signalscope)
        {
            NewViewmodelArm(tool)?.SetArmData("Signalscope");
            return;
        }

        if (tool is NomaiTranslator)
        {
            NewViewmodelArm(tool)?.SetArmData("NomaiTranslator");
            return;
        }
    }

    internal static void OnPickUpItem(OWItem item)
    {
        if (!Config.EnableViewmodelArms) return;

        // rotate lantern to put it in better position for viewmodel arm
        if (item.GetItemType() == ItemType.Lantern)
            item.transform.localEulerAngles = new Vector3(0f, 327f, 0f);

        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (item.transform.Find("ViewmodelArm")) return;

        switch (item.GetItemType())
        {
            case ItemType.SharedStone:
                if (item is SharedStone)
                    NewViewmodelArm(item)?.SetArmData("SharedStone");
                break;

            case ItemType.Scroll:
                if (item is ScrollItem)
                {
                    switch (item.name)
                    {
                        case "Prefab_NOM_Scroll_egg":
                            NewViewmodelArm(item)?.SetArmData("Scroll_Egg");
                            break;

                        case "Prefab_NOM_Scroll_Jeff":
                            NewViewmodelArm(item)?.SetArmData("Scroll_Jeff");
                            break;

                        default:
                            NewViewmodelArm(item)?.SetArmData("Scroll");
                            break;
                    }
                }
                break;

            case ItemType.ConversationStone:
                if (item is NomaiConversationStone solanumStone)
                {
                    if (solanumStone._word == NomaiWord.Identify || solanumStone._word == NomaiWord.Explain)
                        NewViewmodelArm(item)?.SetArmData("ConversationStone_Big");
                    else
                        NewViewmodelArm(item)?.SetArmData("ConversationStone");
                }
                break;

            case ItemType.WarpCore:
                if (item is WarpCoreItem warpCore)
                {
                    if (warpCore._warpCoreType == WarpCoreType.Vessel || warpCore._warpCoreType == WarpCoreType.VesselBroken)
                        NewViewmodelArm(item)?.SetArmData("WarpCore");
                    else
                        NewViewmodelArm(item)?.SetArmData("WarpCore_Simple");
                }
                break;

            case ItemType.Lantern:
                if (item is SimpleLanternItem)
                    NewViewmodelArm(item)?.SetArmData("Lantern");
                break;

            case ItemType.SlideReel:
                if (item is SlideReelItem)
                    NewViewmodelArm(item)?.SetArmData("SlideReel");
                break;

            case ItemType.DreamLantern:
                if (item is DreamLanternItem dreamLantern)
                {
                    switch (dreamLantern._lanternType)
                    {
                        case DreamLanternType.Nonfunctioning:
                            NewViewmodelArm(item)?.SetArmData("DreamLantern_Nonfunctioning");
                            break;

                        case DreamLanternType.Malfunctioning:
                            NewViewmodelArm(item)?.SetArmData("DreamLantern_Malfunctioning");
                            break;

                        default:
                            NewViewmodelArm(item)?.SetArmData("DreamLantern");
                            break;
                    }
                }
                break;

            case ItemType.VisionTorch:
                if (item is VisionTorchItem)
                    NewViewmodelArm(item)?.SetArmData("VisionTorch");
                break;
        }
    }

    internal static void LoadAssetIfNull()
    {
        if (s_viewmodelArmAsset == null)
            s_viewmodelArmAsset = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm").LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        LoadAssetIfNull();

        var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
        viewmodelArm.name = "ViewmodelArm";
        viewmodelArm.transform.parent = parent;
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;

        return viewmodelArm;
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
        if (!Config.EnableViewmodelArms)
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
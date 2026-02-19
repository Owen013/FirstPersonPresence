using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

public class ViewmodelArm : MonoBehaviour
{
    private static AssetBundle s_viewmodelArmAssetBundle;

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

    private GameObject _playerModelArmNoSuit;

    private GameObject _playerModelArmSuit;

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool)
    {
        var viewmodelArm = NewViewmodelArm(playerTool.transform);
        viewmodelArm._playerTool = playerTool;

        viewmodelArm.SetArmData(playerTool.name);

        return viewmodelArm;
    }

    public static ViewmodelArm NewViewmodelArm(OWItem owItem)
    {
        var viewmodelArm = NewViewmodelArm(owItem.transform);
        owItem.onPickedUp.AddListener((_) => viewmodelArm.gameObject.SetActive(true));

        string armDataID = ArmData.TryGetArmDataID(owItem);
        if (armDataID != null)
            viewmodelArm.SetArmData(armDataID);

        return viewmodelArm;
    }

    public void SetArmData(ArmData armData)
    {
        transform.localPosition = armData.armOffsetPos;
        transform.localEulerAngles = armData.armOffsetRot;
        transform.localScale = 0.1f * armData.armScale * Vector3.one;
        SetShader(armData.armShader);
        SetBoneEulers(armData.boneEulers);
    }

    public void SetArmData(string armDataID)
    {
        var armData = ArmData.GetArmData(armDataID);
        if (armData != null)
            SetArmData(armData);
    }

    public void OutputArmData()
    {
        string output = "  [ARMDATA NAME HERE] {\n";

        var armPos = transform.localPosition;
        output += "    \"arm_offset_pos\": { " + $"\"x\": {armPos.x}, \"y\":  {armPos.y}, \"z\": {armPos.z}" + " },\n";
        var armRot = transform.localEulerAngles;
        output += "    \"arm_offset_rot\": { " + $"\"x\": {armRot.x}, \"y\":  {armRot.y}, \"z\": {armRot.z}" + " },\n";
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
        s_viewmodelArmAssetBundle = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm");
    }

    internal static void OnEquipTool(PlayerTool tool)
    {
        // don't try to add viewmodel arm if disabled in config
        if (!Config.EnableViewmodelArms || !ArmData.ArmDataExists(tool.name)) return;

        // check for existing arm and enable if found (PlayerTool has no event for tool being equipped, so this is required)
        var existingArm = tool.transform.Find("ViewmodelArm");
        if (existingArm != null)
        {
            existingArm.gameObject.SetActive(true);
            return;
        }

        NewViewmodelArm(tool);
    }

    internal static void OnPickUpItem(OWItem item)
    {
        if (!Config.EnableViewmodelArms || !ArmData.ArmDataExists(ArmData.TryGetArmDataID(item))) return;

        bool isCompatibleItem = ItemUtils.IsBaseGameItem(item) || ItemUtils.IsTSTAItem(item);
        if (isCompatibleItem)
        {
            ApplyItemAdjustments(item);
            if (item.transform.Find("ViewmodelArm") == null)
                NewViewmodelArm(item);
        }
    }

    internal static void OnDropItem(OWItem item) =>
        item.transform.Find("ViewmodelArm")?.gameObject.SetActive(false);

    private static void ApplyItemAdjustments(OWItem item)
    {
        if (ItemUtils.IsBaseGameItem(item) && item.GetItemType() == ItemType.Lantern)
            item.transform.localEulerAngles = new Vector3(0f, 327f, 0f);
        else if (ItemUtils.IsTSTAItem(item) && item.GetDisplayName() == "Skull")
            ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => item.transform.localScale = 0.6f * Vector3.one);
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        if (s_viewmodelArmAssetBundle == null)
            LoadAssetBundle();
        var viewmodelArmAsset = s_viewmodelArmAssetBundle.LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
        var viewmodelArm = Instantiate(viewmodelArmAsset).GetComponent<ViewmodelArm>();
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

        // if using the viewmodel shader, the prepass meshes must be enabled to prevent viewmodel arms from appearing behind things
        bool isViewmodel = shaderName == "Outer Wilds/Utility/View Model" || shaderName == "Outer Wilds/Utility/View Model (Cutoff)";
        _prePassNoSuit.gameObject.SetActive(isViewmodel);
        _prePassSuit.gameObject.SetActive(isViewmodel);
        if (isViewmodel)
        {
            // grab the ingame viewmodel prepass shader (the prefab one can't work properly)
            var prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
            _prePassNoSuit.materials[0].shader = prepassShader;
            _prePassNoSuit.materials[1].shader = prepassShader;
            _prePassSuit.material.shader = prepassShader;
        }
    }

    private void SetBoneEulers(Dictionary<string, Vector3> boneEulersDict)
    {
        foreach (var boneEulers in boneEulersDict)
            _bones[boneEulers.Key].localEulerAngles = boneEulers.Value;
    }

    private void Awake()
    {
        // grab the bones that matter
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

    private void Start()
    {
        var player = Locator.GetPlayerController().transform;
        _playerModelArmNoSuit = player.transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerModelArmSuit = player.transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
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

        _armMeshNoSuit.gameObject.SetActive(_playerModelArmNoSuit.activeInHierarchy);
        _armMeshSuit.gameObject.SetActive(_playerModelArmSuit.activeInHierarchy);
    }
}
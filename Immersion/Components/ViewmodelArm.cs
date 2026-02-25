using Immersion.Objects;
using OWML.Utils;
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

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool) =>
        NewViewmodelArm(playerTool.transform);

    public static ViewmodelArm NewViewmodelArm(OWItem owItem) =>
        NewViewmodelArm(owItem.transform);

    public void SetArmPose(ArmPose armPose)
    {
        transform.localPosition = armPose.armLocalPosition;
        transform.localEulerAngles = armPose.armLocalEulerAngles;
        transform.localScale = 0.1f * armPose.armScale * Vector3.one;
        SetShader(armPose.armShader);
        SetBonesEulerAngles(armPose.bonesLocalEulerAngles);
    }

    public void SetArmPose(string armPoseID)
    {
        var armPose = ArmPose.GetArmPose(armPoseID);
        if (armPose != null)
            SetArmPose(armPose);
    }

    public void OutputArmPose()
    {
        var armPos = transform.localPosition;
        var armRot = transform.localEulerAngles;

        string boneEulers = "";
        foreach (var keyValuePair in _bones)
        {
            var eulers = keyValuePair.Value.localEulerAngles;
            boneEulers += $"      \"{keyValuePair.Key}\": {{ \"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z} }},\n";
        }

        string output = "  [ARM POSE ID HERE] {\n" +
            $"    \"arm_local_position\": {{ \"x\": {armPos.x}, \"y\": {armPos.y}, \"z\": {armPos.z} }},\n" +
            $"    \"arm_local_euler_angles\": {{ \"x\": {armRot.x}, \"y\": {armRot.y}, \"z\": {armRot.z} }},\n" +
            $"    \"arm_scale\": {10f * transform.localScale.x},\n" +
            $"    \"arm_shader\": \"{_armMeshNoSuit.material.shader.name}\",\n" +
            "    \"bones_local_euler_angles\": {\n" +
            boneEulers +
            "    }\n  }";

        ModMain.Log(output);
    }

    internal static void LoadAsset()
    {
        var assetBundle = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm");
        s_viewmodelArmAsset = assetBundle.LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
    }

    internal static void OnEquipTool(PlayerTool tool)
    {
        // don't try to add viewmodel arm if disabled in config
        if (!Config.EnableViewmodelArms || !ArmPose.ArmPoseExists(tool.name)) return;

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
        if (!Config.EnableViewmodelArms || !ArmPose.ArmPoseExists(ArmPose.TryGetArmPoseID(item))) return;

        ApplyItemAdjustments(item);
        if (item.transform.Find("ViewmodelArm") == null)
            NewViewmodelArm(item);
    }

    private static void ApplyItemAdjustments(OWItem item)
    {
        switch (item.GetItemType().GetName())
        {
            case "Lantern":
                item.transform.localEulerAngles = new Vector3(0f, 327f, 0f);
                return;
            case "GhostbirdSkull":
                ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => item.transform.localScale = 0.6f * Vector3.one);
                return;
        }
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
        viewmodelArm.name = "ViewmodelArm";
        viewmodelArm.transform.parent = parent;
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;
        viewmodelArm.SetShader("Standard");

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

    private void SetBonesEulerAngles(Dictionary<string, Vector3> bonesEulerAngles)
    {
        foreach (var boneEulerAngles in bonesEulerAngles)
            _bones[boneEulerAngles.Key].localEulerAngles = boneEulerAngles.Value;
    }

    private void Awake()
    {
        // grab the bones that matter
        _bones = new Dictionary<string, Transform>
        {
            ["shoulder"] = _armMeshNoSuit.bones[5],
            ["elbow"] = _armMeshNoSuit.bones[6],
            ["wrist"] = _armMeshNoSuit.bones[7],
            ["finger_01_01"] = _armMeshNoSuit.bones[8],
            ["finger_01_02"] = _armMeshNoSuit.bones[9],
            ["finger_01_03"] = _armMeshNoSuit.bones[10],
            ["finger_01_04"] = _armMeshNoSuit.bones[11],
            ["finger_02_01"] = _armMeshNoSuit.bones[12],
            ["finger_02_02"] = _armMeshNoSuit.bones[13],
            ["finger_02_03"] = _armMeshNoSuit.bones[14],
            ["finger_02_04"] = _armMeshNoSuit.bones[15],
            ["thumb_01"] = _armMeshNoSuit.bones[16],
            ["thumb_02"] = _armMeshNoSuit.bones[17],
            ["thumb_03"] = _armMeshNoSuit.bones[18],
            ["thumb_04"] = _armMeshNoSuit.bones[19]
        };
    }

    private void Start()
    {
        _playerTool = transform.parent.GetComponent<PlayerTool>();
        if (_playerTool != null)
            SetArmPose(_playerTool.name);
        else
        {
            _owItem = transform.parent.GetComponent<OWItem>();
            _owItem.onPickedUp.AddListener((_) => gameObject.SetActive(true));
            _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();

            string armPoseID = ArmPose.TryGetArmPoseID(_owItem);
            if (armPoseID != null)
                SetArmPose(armPoseID);
        }
        _playerModelArmNoSuit = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerModelArmSuit = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
    }

    private void LateUpdate()
    {
        if (!Config.EnableViewmodelArms)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_playerTool != null && ((!_playerTool.IsEquipped() && !_playerTool.IsPuttingAway()) || OWInput.IsInputMode(InputMode.ShipCockpit)))
        {
            gameObject.SetActive(false);
            return;
        }
        else if (_owItem != null && _itemCarryTool.GetHeldItem() != _owItem)
        {
            gameObject.SetActive(false);
            return;
        }

        _armMeshNoSuit.gameObject.SetActive(_playerModelArmNoSuit.activeInHierarchy);
        _armMeshSuit.gameObject.SetActive(_playerModelArmSuit.activeInHierarchy);
    }
}
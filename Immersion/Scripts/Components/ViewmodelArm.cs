using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Scripts.Components;

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

    /// <summary>
    /// Creates a new ViewmodelArm for the given PlayerTool.
    /// </summary>
    /// <param name="playerTool">The PlayerTool to add a ViewmodelArm to.</param>
    /// <returns>The new ViewmodelArm.</returns>
    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool) =>
        NewViewmodelArm(playerTool.transform);

    /// <summary>
    /// Creates a new ViewmodelArm for the given OWItem.
    /// </summary>
    /// <param name="owItem">The OWItem to add a ViewmodelArm to.</param>
    /// <returns>The new ViewmodelArm.</returns>
    public static ViewmodelArm NewViewmodelArm(OWItem owItem) =>
        NewViewmodelArm(owItem.transform);

    /// <summary>
    /// Applies a position, rotation, scale, shader, and pose from an ArmData to this ViewmodelArm.
    /// </summary>
    /// <param name="armData">The ArmData to apply to this ViewmodelArm.</param>
    public void SetArmData(ArmData armData)
    {
        transform.localPosition = armData.armLocalPosition;
        transform.localEulerAngles = armData.armLocalEulerAngles;
        transform.localScale = 0.1f * armData.armScale * Vector3.one;
        SetShader(armData.armShader);
        SetBonesEulerAngles(armData.bonesLocalEulerAngles);
    }

    /// <summary>
    /// Applies a position, rotation, scale, shader, and pose from an ArmData to this ViewmodelArm.
    /// </summary>
    /// <param name="armDataId">The ID of the ArmData to apply to this ViewmodelArm.</param>
    public void SetArmData(string armDataId)
    {
        var armData = ArmData.Find(armDataId);
        if (armData != null)
            SetArmData(armData);
    }

    /// <summary>
    /// Outputs this ViewmodelArm's information in JSON format.
    /// </summary>
    public void OutputArmData()
    {
        var armPos = transform.localPosition;
        var armRot = transform.localEulerAngles;

        string boneEulers = "";
        foreach (var keyValuePair in _bones)
        {
            var eulers = keyValuePair.Value.localEulerAngles;
            boneEulers += $"      \"{keyValuePair.Key}\": {{ \"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z} }},\n";
        }

        string output = "  [ARM DATA ID HERE] {\n" +
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

    internal static void OnEquipTool(PlayerTool playerTool)
    {
        if (Config.EnableViewmodelArms && ArmData.Exists(playerTool.name))
        {
            // check for existing arm and enable if found (PlayerTool has no event for being equipped, so this is required)
            var existingArm = playerTool.transform.Find("ViewmodelArm");
            if (existingArm != null)
            {
                existingArm.gameObject.SetActive(true);
                return;
            }

            NewViewmodelArm(playerTool);
        }
    }

    internal static void OnPickUpItem(OWItem owItem)
    {
        if (Config.EnableViewmodelArms && ArmData.Exists(ArmData.FindArmDataIdOfItem(owItem)) && owItem.transform.Find("ViewmodelArm") == null)
            NewViewmodelArm(owItem);

        // some items need to be adjusted
        switch (owItem.GetItemType().GetName())
        {
            case "Lantern":
                owItem.transform.localEulerAngles = new Vector3(0f, 327f, 0f);
                break;
            case "GhostbirdSkull":
                ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => owItem.transform.localScale = 0.6f * Vector3.one);
                break;
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
        if (string.IsNullOrEmpty(shaderName))
        {
            ModMain.Log("No shaderName provided for ViewmodelArm.SetShader", MessageType.Error);
            return;
        }

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
            SetArmData(_playerTool.name);
        else
        {
            _owItem = transform.parent.GetComponent<OWItem>();
            _owItem.onPickedUp.AddListener((_) => gameObject.SetActive(true));
            _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();

            string armDataId = ArmData.FindArmDataIdOfItem(_owItem);
            if (armDataId != null)
                SetArmData(armDataId);
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
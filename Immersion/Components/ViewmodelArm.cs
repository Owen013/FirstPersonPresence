using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;
using static Immersion.Immersion;

namespace Immersion.Components;

public class ViewmodelArm : MonoBehaviour
{
    private static GameObject s_viewmodelArmAsset;

    [SerializeField]
    private SkinnedMeshRenderer _noSuitMesh = default;

    [SerializeField]
    private SkinnedMeshRenderer _noSuitMeshPrepass = default;

    [SerializeField]
    private SkinnedMeshRenderer _suitMesh = default;

    [SerializeField]
    private SkinnedMeshRenderer _suitMeshPrepass = default;

    private Dictionary<string, Transform> _bones;

    private ViewmodelArmType _type;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    private ItemTool _itemCarryTool;

    private GameObject _playerNoSuitMesh;

    private GameObject _playerSuitMesh;

    private enum ViewmodelArmType
    {
        Invalid = 0,
        PlayerTool = 1,
        OWItem = 2
    }

    /// <summary>
    /// Creates a new Viewmodel Arm for a PlayerTool.
    /// </summary>
    /// <param name="playerTool">The PlayerTool to attach the new Viewmodel Arm to.</param>
    /// <returns>The new ViewmodelArm.</returns>
    public static ViewmodelArm New(PlayerTool playerTool)
    {
        var newViewmodelArm = NewViewmodelArm(playerTool.transform, ArmData.Find(playerTool));
        newViewmodelArm._type = ViewmodelArmType.PlayerTool;
        newViewmodelArm._playerTool = playerTool;
        return newViewmodelArm;
    }

    /// <summary>
    /// Creates a new Viewmodel Arm for an OWItem.
    /// </summary>
    /// <param name="owItem">The OWItem to attach the new Viewmodel Arm to.</param>
    /// <returns>The new ViewmodelArm.</returns>
    public static ViewmodelArm New(OWItem owItem)
    {
        var newViewmodelArm = NewViewmodelArm(owItem.transform, ArmData.Find(owItem));
        newViewmodelArm._type = ViewmodelArmType.OWItem;
        newViewmodelArm._owItem = owItem;
        newViewmodelArm._owItem.onPickedUp.AddListener((_) => newViewmodelArm.gameObject.SetActive(true));
        newViewmodelArm._itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();

        return newViewmodelArm;
    }

    /// <summary>
    /// Changes the Shaders of the Viewmodel Arm meshes.
    /// </summary>
    /// <param name="shader">The Shader to use for the Viewmodel Arm meshes.</param>
    public void SetShader(Shader shader)
    {
        _noSuitMesh.materials[0].shader = shader;
        _noSuitMesh.materials[1].shader = shader;
        _suitMesh.material.shader = shader;

        // If using the viewmodel shader, the prepass meshes must be enabled to prevent viewmodel arms from being drawn
        // underneath other objects.
        bool isViewmodel = shader.name == "Outer Wilds/Utility/View Model"
                        || shader.name == "Outer Wilds/Utility/View Model (Cutoff)";
        _noSuitMeshPrepass.gameObject.SetActive(isViewmodel);
        _suitMeshPrepass.gameObject.SetActive(isViewmodel);
    }

    /// <summary>
    /// Changes the Shaders of the Viewmodel Arm meshes.
    /// </summary>
    /// <param name="shaderName">The name of the Shader to use for the Viewmodel Arm meshes.</param>
    public void SetShader(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            ModConsole.WriteLine($"\"{shaderName}\" is not a valid shader.", MessageType.Error);
            return;
        }

        SetShader(shader);
    }

    /// <summary>
    /// Sets the euler angles of the Viewmodel Arm's bones.
    /// </summary>
    /// <param name="boneEulers">A dictionary of Vector3 values keyed on the names of the bones.</param>
    public void SetBoneEulers(Dictionary<string, Vector3> boneEulers)
    {
        foreach (var boneEuler in boneEulers)
            _bones[boneEuler.Key].localEulerAngles = boneEuler.Value;
    }

    /// <summary>
    /// Sets the position, rotation, scale, shader, and bone eulers of the Viewmodel Arm to those defined by an ArmData
    /// object.
    /// </summary>
    /// <param name="armData">The ArmData to apply to this ViewmodelArm.</param>
    public void ApplyArmData(ArmData armData)
    {
        transform.localPosition = armData.armPosition;
        transform.localEulerAngles = armData.armRotation;
        transform.localScale = 0.1f * armData.armScale * Vector3.one;
        SetShader(armData.armShader);
        SetBoneEulers(armData.boneEulers);
    }

    /// <summary>
    /// Sets the position, rotation, scale, shader, and bone eulers of the Viewmodel Arm to those defined by an ArmData
    /// object.
    /// </summary>
    /// <param name="armDataId">The ID of the ArmData to apply to this ViewmodelArm.</param>
    public void ApplyArmData(string armDataId)
    {
        var armData = ArmData.Find(armDataId);
        if (armData != null)
            ApplyArmData(armData);
    }

    /// <summary>
    /// Outputs this ViewmodelArm's information in a format that can be used in the ArmData JSON.
    /// </summary>
    public void OutputArmDataJSON()
    {
        Vector3 armPos = transform.localPosition;
        Vector3 armRot = transform.localEulerAngles;

        string output = $"    \"[ARM DATA ID HERE]\" {{\n";

        string localPosJSON = $"\"x\": {armPos.x}, \"y\": {armPos.y}, \"z\": {armPos.z}";
        output += $"        \"arm_local_position\": {{ {localPosJSON} }},\n";

        string localRotJSON = $"\"x\": {armRot.x}, \"y\": {armRot.y}, \"z\": {armRot.z}";
        output += $"        \"arm_local_euler_angles\": {{ {localRotJSON} }},\n";

        output += $"        \"arm_scale\": {10f * transform.localScale.x},\n";
        output += $"        \"arm_shader\": \"{_noSuitMesh.material.shader.name}\",\n";
        output += $"        \"bones_local_euler_angles\": {{\n";

        int i = 0;
        foreach (var keyValuePair in _bones)
        {
            Vector3 eulers = keyValuePair.Value.localEulerAngles;
            string eulersJSON = $"\"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z}";
            output += $"            \"{keyValuePair.Key}\": {{ {eulersJSON} }}";
            if (i < _bones.Count - 1)
                output += ',';
            output += '\n';
            i++;
        }

        output += $"        }}\n    }}";

        ModConsole.WriteLine(output);
    }

    public static void LoadAsset()
    {
        var assetBundle = ModAssets.LoadBundle("AssetBundles/viewmodelarm");
        s_viewmodelArmAsset = assetBundle.LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
    }

    public static void OnEquipTool(PlayerTool playerTool)
    {
        if (Config.EnableViewmodelArms && ArmData.Exists(playerTool))
        {
            // If there is already a viewmodel arm on this tool, just enable it.
            var existingArm = playerTool.transform.Find("ViewmodelArm");
            if (existingArm != null)
            {
                existingArm.gameObject.SetActive(true);
                return;
            }

            ViewmodelArm.New(playerTool);
        }
    }

    public static void OnPickUpItem(OWItem owItem)
    {
        if (Config.EnableViewmodelArms && ArmData.Exists(owItem) && owItem.transform.Find("ViewmodelArm") == null)
            ViewmodelArm.New(owItem);

        // Certain items need to be moved slightly to reduce near-clipping of viewmodel arm.
        switch (owItem.GetItemType().GetName())
        {
            case "ConversationStone":
                owItem.transform.localPosition += 0.2f * Vector3.forward;
                break;
            case "Lantern":
                owItem.transform.localEulerAngles = new Vector3(0f, 327f, 0f);
                break;
            case "GhostbirdSkull":
                ModEvents.Unity.FireOnNextUpdate(() =>
                    owItem.transform.localScale = 0.6f * Vector3.one);
                break;
        }
    }

    private static ViewmodelArm NewViewmodelArm(Transform parent, ArmData armData = null)
    {
        var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
        viewmodelArm.name = "ViewmodelArm";
        viewmodelArm.transform.parent = parent;
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;

        // get the ingame shaders
        viewmodelArm.SetShader("Standard");
        var prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
        viewmodelArm._noSuitMeshPrepass.materials[0].shader = prepassShader;
        viewmodelArm._noSuitMeshPrepass.materials[1].shader = prepassShader;
        viewmodelArm._suitMeshPrepass.material.shader = prepassShader;

        if (armData != null)
            viewmodelArm.ApplyArmData(armData);

        return viewmodelArm;
    }

    private void Awake()
    {
        _bones = new Dictionary<string, Transform>
        {
            ["shoulder"] = _noSuitMesh.bones[5],
            ["elbow"] = _noSuitMesh.bones[6],
            ["wrist"] = _noSuitMesh.bones[7],
            ["finger_01_01"] = _noSuitMesh.bones[8],
            ["finger_01_02"] = _noSuitMesh.bones[9],
            ["finger_01_03"] = _noSuitMesh.bones[10],
            ["finger_01_04"] = _noSuitMesh.bones[11],
            ["finger_02_01"] = _noSuitMesh.bones[12],
            ["finger_02_02"] = _noSuitMesh.bones[13],
            ["finger_02_03"] = _noSuitMesh.bones[14],
            ["finger_02_04"] = _noSuitMesh.bones[15],
            ["thumb_01"] = _noSuitMesh.bones[16],
            ["thumb_02"] = _noSuitMesh.bones[17],
            ["thumb_03"] = _noSuitMesh.bones[18],
            ["thumb_04"] = _noSuitMesh.bones[19]
        };
    }

    private void Start()
    {
        var playerTransform = Locator.GetPlayerBody().transform;
        _playerNoSuitMesh = playerTransform.Find(
            "Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm")
            .gameObject;
        _playerSuitMesh = playerTransform.Find(
            "Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm")
            .gameObject;
    }

    private void LateUpdate()
    {
        if (!Config.EnableViewmodelArms)
        {
            gameObject.SetActive(false);
            return;
        }

        switch (_type)
        {
            case ViewmodelArmType.PlayerTool:
                bool isToolAway = !_playerTool.IsEquipped() && !_playerTool.IsPuttingAway();
                if (isToolAway || OWInput.IsInputMode(InputMode.ShipCockpit))
                {
                    gameObject.SetActive(false);
                    return;
                }
                break;

            case ViewmodelArmType.OWItem:
                if (_itemCarryTool.GetHeldItem() != _owItem)
                {
                    gameObject.SetActive(false);
                    return;
                }
                break;
        }

        _noSuitMesh.gameObject.SetActive(_playerNoSuitMesh.activeInHierarchy);
        _suitMesh.gameObject.SetActive(_playerSuitMesh.activeInHierarchy);
    }
}
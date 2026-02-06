using HarmonyLib;
using Immersion.Objects;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    public static readonly string[] s_boneNames =
    [
        "Spine_01",
        "Spine_02",
        "Spine_Top",
        "LF_Arm_Clavicle",
        "RT_Arm_Clavicle",
        "RT_Arm_Shoulder",
        "RT_Arm_Elbow",
        "RT_Arm_Wrist",
        "RT_Finger_01_01",
        "RT_Finger_01_02",
        "RT_Finger_01_03",
        "RT_Finger_01_04",
        "RT_Finger_02_01",
        "RT_Finger_02_02",
        "RT_Finger_02_03",
        "RT_Finger_02_04",
        "RT_Thumb_01_01",
        "RT_Thumb_01_02",
        "RT_Thumb_01_03",
        "RT_Thumb_01_04",
        "Neck",
        "Pack"
    ];

    public static readonly int[] s_boneParentIndices =
    [
        -1,
        0,
        1,
        2,
        2,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        7,
        12,
        13,
        14,
        7,
        16,
        17,
        18,
        2,
        2
    ];

    private static GameObject s_armTemplate;

    private static GameObject s_playerRightArmNoSuit;

    private static GameObject s_playerRightArmSuit;

    private static Dictionary<string, Shader> s_armShaders;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    private ItemTool _itemCarryTool;

    private GameObject _viewmodelArmNoSuit;

    private GameObject _viewmodelArmSuit;

    private Transform[] _bones;

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool)
    {
        return NewViewmodelArm(playerTool, null);
    }

    public static ViewmodelArm NewViewmodelArm(OWItem owItem)
    {
        return NewViewmodelArm(null, owItem);
    }

    // this method should not be used except by the above two methods
    private static ViewmodelArm NewViewmodelArm(PlayerTool playerTool, OWItem owItem)
    {
        if (s_armTemplate == null)
        {
            ModMain.Instance.ModHelper.Console.WriteLine("Cannot create ViewmodelArm right now; template has not been created", MessageType.Warning);
            return null;
        }

        var armObject = Instantiate(s_armTemplate);
        armObject.name = "ViewmodelArm";
        armObject.SetActive(true);

        // add component and initialize fields
        var viewmodelArm = armObject.AddComponent<ViewmodelArm>();
        viewmodelArm._viewmodelArmNoSuit = armObject.transform.Find("ArmMesh_NoSuit").gameObject;
        viewmodelArm._viewmodelArmSuit = armObject.transform.Find("ArmMesh_Suit").gameObject;
        viewmodelArm._bones = viewmodelArm._viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>().bones;

        // Move to transform
        if (playerTool != null)
        {
            viewmodelArm.transform.parent = playerTool.transform;
            viewmodelArm._playerTool = playerTool;
        }
        else
        {
            viewmodelArm.transform.parent = owItem.transform;
            viewmodelArm._owItem = owItem;
            owItem.onPickedUp += (item) =>
            {
                viewmodelArm.gameObject.SetActive(true);
            };
        }

        var camera = Locator.GetPlayerCamera();
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localEulerAngles = Vector3.zero;
        viewmodelArm.transform.localScale = 0.1f * Vector3.one;

        return viewmodelArm;
    }

    public void SetArmData(string itemName)
    {
        var armData = ArmData.GetArmData(itemName);
        if (armData == null) return;

        SetBonePoses(armData.bonePositions, armData.boneEulers);
        transform.localScale = 0.1f * Vector3.one * armData.scale;
        SetShader(armData.shaderName);
    }

    public void OutputArmData()
    {
        string output = "Bone Positions:\n";
        for (int i = 0; i < _bones.Length; i++)
        {
            var bonePositions = _bones[i].localPosition;
            output += $"[ {Mathf.Round(bonePositions.x * 1000f) / 1000f}, {Mathf.Round(bonePositions.y * 1000f) / 1000f}, {Mathf.Round(bonePositions.z * 1000f) / 1000f} ],\n";
        }

        output += "\nBone Eulers:\n";
        for (int i = 0; i < _bones.Length; i++)
        {
            var boneEulers = _bones[i].localEulerAngles;
            output += $"[ {Mathf.Round(boneEulers.x * 100f) / 100f}, {Mathf.Round(boneEulers.y * 100f) / 100f}, {Mathf.Round(boneEulers.z * 100f) / 100f} ],\n";
        }

        output += $"\nArm Scale: {transform.localScale.x * 10f}\n";
        output += $"\nShader: \"{_viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>().materials[0].shader.name}\"";

        ModMain.Instance.ModHelper.Console.WriteLine(output);
    }

    public void ResetArmTransform()
    {
        var camera = Locator.GetPlayerCamera();
        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;
        transform.localScale = 0.1f * Vector3.one;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Start))]
    private static void PlayerAnimController_Start_Postfix(PlayerAnimController __instance)
    {
        // create viewmodel arm template
        var armTemplate = new GameObject("ViewmodelArmTemplate");
        armTemplate.SetActive(false);

        // create rig
        var rig = new GameObject("Rig").transform;
        rig.parent = armTemplate.transform;
        var bones = new Transform[22];
        for (int i = 0; i < bones.Length; i++)
        {
            bones[i] = new GameObject(s_boneNames[i]).transform;
            int parentIndex = s_boneParentIndices[i];
            if (parentIndex == -1)
                bones[i].parent = rig;
            else
                bones[i].parent = bones[parentIndex];
        }

        // set up arm meshes and set new bones list
        var noSuitMesh = Instantiate(__instance.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm")).GetComponent<SkinnedMeshRenderer>();
        noSuitMesh.name = "ArmMesh_NoSuit";
        noSuitMesh.transform.parent = armTemplate.transform;
        noSuitMesh.gameObject.layer = 27;
        noSuitMesh.updateWhenOffscreen = true;
        noSuitMesh.rootBone = bones[0];
        noSuitMesh.bones = bones;

        // suit mesh uses different bones for some forsaken reason
        var suitMesh = Instantiate(__instance.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm")).GetComponent<SkinnedMeshRenderer>();
        suitMesh.name = "ArmMesh_Suit";
        suitMesh.transform.parent = armTemplate.transform;
        suitMesh.gameObject.layer = 27;
        suitMesh.updateWhenOffscreen = true;
        suitMesh.rootBone = bones[4];
        suitMesh.bones =
        [
            bones[3],
            bones[4],
            bones[5],
            bones[6],
            bones[7],
            bones[8],
            bones[9],
            bones[10],
            bones[12],
            bones[13],
            bones[14],
            bones[16],
            bones[17],
            bones[18]
        ];

        // arm template is finished
        s_armTemplate = armTemplate;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance)
    {
        // don't try to add viewmodel arm if disabled in config or if this tool already has one
        if (!ModMain.Instance.EnableViewmodelHands) return;

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
        if (!ModMain.Instance.EnableViewmodelHands || __instance.transform.Find("ViewmodelArm")) return;

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

    private void SetBonePoses(Vector3[] positions, Vector3[] eulers)
    {
        for (int i = 0; i < _bones.Length; i++)
        {
            _bones[i].localPosition = positions[i];
            _bones[i].localEulerAngles = eulers[i];
        }
    }

    private void SetShader(string shaderName)
    {
        if (shaderName == "") return;

        Shader shader;
        if (!s_armShaders.ContainsKey(shaderName))
        {
            shader = Shader.Find(shaderName);
            if (shader == null)
            {
                ModMain.Instance.ModHelper.Console.WriteLine($"Shader \"{shaderName}\" not found", MessageType.Error);
                return;
            }

            s_armShaders.Add(shaderName, shader);
        }
        else
        {
            shader = s_armShaders[shaderName];
        }

        var noSuitMesh = _viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>();
        noSuitMesh.materials[0].shader = shader;
        noSuitMesh.materials[1].shader = shader;
        _viewmodelArmSuit.GetComponent<SkinnedMeshRenderer>().material.shader = shader;
    }

    private void Awake()
    {
        _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();

        if (s_armShaders == null)
            s_armShaders = [];

        if (s_playerRightArmNoSuit == null || s_playerRightArmSuit == null)
        {
            var player = Locator.GetPlayerController().transform;
            s_playerRightArmNoSuit = player.transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
            s_playerRightArmSuit = player.transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        }
    }

    private void LateUpdate()
    {
        if (!ModMain.Instance.EnableViewmodelHands)
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

        _viewmodelArmNoSuit.SetActive(s_playerRightArmNoSuit.activeInHierarchy);
        _viewmodelArmSuit.SetActive(s_playerRightArmSuit.activeInHierarchy);
    }
}
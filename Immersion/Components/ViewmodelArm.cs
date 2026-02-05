using HarmonyLib;
using Immersion.Objects;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
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

    public enum ArmShader
    {
        Standard,
        Viewmodel,
        ViewmodelCutoff
    }

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
        viewmodelArm._viewmodelArmNoSuit = armObject.transform.Find("player_mesh_noSuit:Player_RightArm").gameObject;
        viewmodelArm._viewmodelArmSuit = armObject.transform.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
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
        viewmodelArm.transform.position = camera.transform.position;
        viewmodelArm.transform.rotation = camera.transform.rotation;
        viewmodelArm.transform.localScale = 0.1f * Vector3.one;
        armObject.transform.Find("Traveller_Rig_v01:Traveller_Spine_01_Jnt").localPosition = new Vector3(-0.2741f, -8f, -0.6957f);

        return viewmodelArm;
    }

    public void SetArmData(string itemName)
    {
        var armData = ArmData.GetArmData(itemName);
        if (armData == null) return;

        SetBoneEulers(armData.boneEulers);
        transform.localPosition = armData.localPosition;
        transform.localEulerAngles = armData.localEulerAngles;
        transform.localScale = 0.1f * Vector3.one * armData.scale;
        SetShader(armData.shaderName);
    }

    public void OutputArmData()
    {
        string output = "Bone Eulers:\n";
        for (int i = 0; i < 15; i++)
        {
            var boneEulers = _bones[i + 5].localRotation.eulerAngles;
            output += $"[ {Mathf.Round(boneEulers.x * 100f) / 100f}, {Mathf.Round(boneEulers.y * 100f) / 100f}, {Mathf.Round(boneEulers.z * 100f) / 100f} ]";
            if (i < 14)
            {
                output += ",\n";
            }
        }

        output += $"\n\nArm Local Position: [ {Mathf.Round(transform.localPosition.x * 1000f) / 1000f}, {Mathf.Round(transform.localPosition.y * 1000f) / 1000f}, {Mathf.Round(transform.localPosition.z * 1000f) / 1000f} ]\n";
        output += $"\nArm Local Rotation: [ {Mathf.Round(transform.localEulerAngles.x * 100f) / 100f}, {Mathf.Round(transform.localEulerAngles.y * 100f) / 100f}, {Mathf.Round(transform.localEulerAngles.z * 100f) / 100f} ]\n";
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

    internal static void CreateArmTemplate()
    {
        var armObject = Instantiate(Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>().gameObject);
        armObject.name = "ViewmodelArmTemplate";

        // PlayerAnimController has to be destroyed first because Animator depends on it
        Destroy(armObject.GetComponent<PlayerAnimController>());
        armObject.DestroyAllComponents<Behaviour>();

        // Set new root bone
        var newRootBone = armObject.transform.Find("Traveller_Rig_v01:Traveller_Trajectory_Jnt/Traveller_Rig_v01:Traveller_ROOT_Jnt/Traveller_Rig_v01:Traveller_Spine_01_Jnt");
        newRootBone.parent = armObject.transform;
        foreach (Transform transform in armObject.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == "Traveller_Rig_v01:Traveller_Trajectory_Jnt" || transform.name == "player_mesh_noSuit:Traveller_HEA_Player" || transform.name == "Traveller_Rig_v01:Traveller" || transform.name == "Traveller_Mesh_v01:Traveller_Geo")
                Destroy(transform.gameObject);
        }

        static void SetUpArmModel(GameObject arm, Transform rootBone)
        {
            var oldParent = arm.transform.parent;
            arm.transform.parent = arm.transform.parent.parent;
            Destroy(oldParent.gameObject);

            arm.layer = 27;
            var renderer = arm.GetComponent<SkinnedMeshRenderer>();
            renderer.updateWhenOffscreen = true;

            // delete unneeded bones
            renderer.rootBone = rootBone;
            var newBones = new List<Transform>();
            for (int i = 0; i < renderer.bones.Length; i++)
            {
                if (/*i <= 19 && */renderer.bones[i] != null)
                {
                    newBones.Add(renderer.bones[i]);
                }
            }
            renderer.bones = newBones.ToArray();
        }

        var noSuitArm = armObject.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        var suitArm = armObject.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        SetUpArmModel(noSuitArm, newRootBone);
        SetUpArmModel(suitArm, newRootBone);

        armObject.SetActive(false);
        s_armTemplate = armObject;
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
                switch (__instance.name)
                {
                    case "Prefab_NOM_Scroll_Jeff":
                        NewViewmodelArm(__instance)?.SetArmData("Scroll_Jeff");
                        break;

                    case "Prefab_NOM_Scroll_egg":
                        NewViewmodelArm(__instance)?.SetArmData("Scroll_Egg");
                        break;

                    default:
                        NewViewmodelArm(__instance)?.SetArmData("Scroll");
                        break;
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

    private void SetBoneEulers(Vector3[] eulers)
    {
        for (int i = 0; i <= 14; i++)
        {
            _bones[i + 5].localEulerAngles = eulers[i];
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
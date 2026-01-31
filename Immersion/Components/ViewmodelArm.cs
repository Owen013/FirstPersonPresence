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

    private static Dictionary<string, Shader> s_armShaders;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    private ItemTool _itemCarryTool;

    private GameObject _playerRightArmNoSuit;

    private GameObject _playerRightArmSuit;

    private GameObject _viewmodelArmNoSuit;

    private GameObject _viewmodelArmSuit;

    private Transform[] _bones;

    public enum ArmShader
    {
        Standard,
        Viewmodel,
        ViewmodelCutoff
    }

    public enum ArmBone
    {
        Shoulder = 5,
        Elbow = 6,
        Wrist = 7,
        Finger01_01 = 8,
        Finger01_02 = 9,
        Finger01_03 = 10,
        Finger01_04 = 11,
        Finger02_01 = 12,
        Finger02_02 = 13,
        Finger02_03 = 14,
        Finger02_04 = 15,
        Thumb_01 = 16,
        Thumb_02 = 17,
        Thumb_03 = 18,
        Thumb_04 = 19,
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

        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;
        viewmodelArm.transform.localScale = 0.1f * Vector3.one;
        armObject.transform.Find("Traveller_Rig_v01:Traveller_Spine_01_Jnt").localPosition = new Vector3(-0.2741f, -8f, -0.6957f);

        return viewmodelArm;
    }

    public void SetArmData(string itemName)
    {
        var armData = ArmData.GetArmData(itemName);
        if (armData == null)
        {
            ModMain.Instance.ModHelper.Console.WriteLine($"No ViewmodelArmData found for {itemName}");
            return;
        }

        SetBoneEulers(armData.boneEulers);
        transform.localPosition = armData.localPosition;
        transform.localScale = 0.1f * Vector3.one * armData.scale;
        SetShader(armData.shaderName);
    }

    public void OutputBoneRotations()
    {
        for (int i = 0; i <= 14; i++)
        {
            var boneEulers = _bones[i + 5].localRotation.eulerAngles;
            ModMain.Instance.ModHelper.Console.WriteLine($"{(ArmBone)(i + 5)}: [ {boneEulers.x}, {boneEulers.y}, {boneEulers.z} ]");
        }
    }

    internal static void OnSceneLoad()
    {
        Transform camera = Locator.GetPlayerCamera().transform;

        CreateArmTemplate();
        ModMain.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
        {
            NewViewmodelArm(camera.Find("Signalscope").GetComponent<PlayerTool>()).SetArmData("Signalscope");
            NewViewmodelArm(camera.Find("ProbeLauncher").GetComponent<PlayerTool>()).SetArmData("ProbeLauncher");
            NewViewmodelArm(camera.Find("NomaiTranslatorProp").GetComponent<PlayerTool>()).SetArmData("Translator");
            NewViewmodelArm(camera.Find("ItemCarryTool").GetComponent<PlayerTool>());
        });
    }

    private static void CreateArmTemplate()
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

        s_armShaders ??= [];

        Shader shader;
        if (!s_armShaders.ContainsKey(shaderName))
        {
            shader = Shader.Find(shaderName);
            if (shader == null)
            {
                ModMain.Instance.ModHelper.Console.WriteLine($"\"{shaderName}\" shader not found", MessageType.Error);
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

        var playerTransform = Locator.GetPlayerController().transform;
        _playerRightArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerRightArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
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
            if (!_playerTool._isEquipped && !_playerTool._isPuttingAway)
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

        _viewmodelArmNoSuit.SetActive(_playerRightArmNoSuit.activeInHierarchy);
        _viewmodelArmSuit.SetActive(_playerRightArmSuit.activeInHierarchy);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void OnEquipTool(PlayerTool __instance)
    {
        var arm = __instance.transform.Find("ViewmodelArm");
        if (arm != null)
        {
            arm.gameObject.SetActive(true);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void ItemPickedUp(OWItem __instance)
    {
        if (ModMain.Instance.TweakItemPos && __instance._type == ItemType.ConversationStone)
            __instance.transform.localPosition = 0.2f * Vector3.forward;

        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (!ModMain.Instance.EnableViewmodelHands || __instance.transform.Find("ViewmodelArm")) return;

        switch (__instance._type)
        {
            case ItemType.SharedStone:
                NewViewmodelArm(__instance).SetArmData("SharedStone");
                break;
            case ItemType.Scroll:
                if (__instance.name == "Prefab_NOM_Scroll_Jeff")
                {
                    // ...
                }
                else
                {
                    // ...
                }
                break;
            case ItemType.ConversationStone:
                switch ((__instance as NomaiConversationStone)._word)
                {
                    case NomaiWord.Identify:
                        // ...
                        break;
                    case NomaiWord.Explain:
                        // ...
                        break;
                    case NomaiWord.Eye:
                        // ...
                        break;
                    default:
                        // ...
                        break;
                }
                break;
            case ItemType.WarpCore:
                switch ((__instance as WarpCoreItem)._warpCoreType)
                {
                    case WarpCoreType.Vessel:
                        // ...
                        break;
                    case WarpCoreType.VesselBroken:
                        // ...
                        break;
                    default:
                        // ...
                        break;
                }
                break;
            case ItemType.Lantern:
                // ...
                break;
            case ItemType.SlideReel:
                // ...
                break;
            case ItemType.DreamLantern:
                if ((__instance as DreamLanternItem)._lanternType == DreamLanternType.Nonfunctioning)
                {

                }
                else
                {

                }
                break;
            case ItemType.VisionTorch:
                // ...
                break;
        }
    }
}
using HarmonyLib;
using Immersion.Objects;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    private static Dictionary<string, Shader> s_armShaders;

    private ToolModeSwapper _toolModeSwapper;

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

    public static ViewmodelArm NewViewmodelArm(PlayerTool tool)
    {
        var playerModelClone = Instantiate(Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>().gameObject);
        playerModelClone.name = "ViewmodelArm";

        // PlayerAnimController has to be destroyed first because Animator depends on it
        Destroy(playerModelClone.GetComponent<PlayerAnimController>());
        playerModelClone.DestroyAllComponents<Behaviour>();

        // Set new root bone
        var newRootBone = playerModelClone.transform.Find("Traveller_Rig_v01:Traveller_Trajectory_Jnt/Traveller_Rig_v01:Traveller_ROOT_Jnt/Traveller_Rig_v01:Traveller_Spine_01_Jnt");
        newRootBone.parent = playerModelClone.transform;
        Destroy(playerModelClone.transform.Find("Traveller_Rig_v01:Traveller_Trajectory_Jnt").gameObject);

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

        var noSuitArm = playerModelClone.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        var suitArm = playerModelClone.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        SetUpArmModel(noSuitArm, newRootBone);
        SetUpArmModel(suitArm, newRootBone);

        // Move to transform
        playerModelClone.transform.parent = tool.transform;
        playerModelClone.transform.localPosition = Vector3.zero;
        playerModelClone.transform.localRotation = Quaternion.identity;
        newRootBone.localPosition = new Vector3(-0.2741f, -8f, -0.6957f);

        // add component and initialize fields
        var viewmodelArm = playerModelClone.AddComponent<ViewmodelArm>();
        viewmodelArm._viewmodelArmNoSuit = playerModelClone.transform.Find("player_mesh_noSuit:Player_RightArm").gameObject;
        viewmodelArm._viewmodelArmSuit = playerModelClone.transform.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        viewmodelArm._bones = noSuitArm.GetComponent<SkinnedMeshRenderer>().bones;

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
        SetScale(armData.scale);
        SetShader(armData.shaderName);
    }

    public void OutputBoneRotations()
    {
        for (int i = 0; i <= 14; i++)
        {
            var bone = _bones[i + 5];
            ModMain.Instance.ModHelper.Console.WriteLine($"{(ArmBone)(i + 5)}: {bone.localRotation.eulerAngles}");
        }
    }

    internal static void OnSceneLoad()
    {
        Transform player = Locator.GetPlayerTransform();

        // create testing signalscope arm
        var testArm = NewViewmodelArm(player.GetComponentInChildren<Signalscope>());
        testArm.SetArmData("Signalscope");
    }

    private void SetBoneEulers(Vector3[] eulers)
    {
        for (int i = 0; i <= 14; i++)
        {
            _bones[i + 5].localEulerAngles = eulers[i];
        }
    }

    private void SetScale(float scale)
    {
        transform.localScale = 0.1f * Vector3.one * scale;
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
        _toolModeSwapper = Locator.GetToolModeSwapper();

        var playerTransform = Locator.GetPlayerController().transform;
        _playerRightArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerRightArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
    }

    private void LateUpdate()
    {
        if (!ModMain.Instance.EnableViewmodelHands || _toolModeSwapper.GetToolMode() == ToolMode.None)
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
}
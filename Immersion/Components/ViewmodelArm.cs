using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    public enum ArmShader
    {
        Standard,
        Viewmodel,
        ViewmodelCutoff
    }

    private static Shader[] s_armShaders;

    private GameObject _playerRightArmNoSuit;

    private GameObject _playerRightArmSuit;

    private GameObject _viewmodelArmNoSuit;

    private GameObject _viewmodelArmSuit;

    private Transform[] _bones;

    public static ViewmodelArm NewViewmodelArm(Transform parent)
    {
        var playerModelClone = Instantiate(Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>().gameObject);

        // Remove all behaviors. PlayerAnimController has to be destroyed first because Animator depends on it
        Destroy(playerModelClone.GetComponent<PlayerAnimController>());
        playerModelClone.DestroyAllComponents<Behaviour>();

        // Set new root bone
        var newRootBone = playerModelClone.transform.Find("Traveller_Rig_v01:Traveller_Trajectory_Jnt/Traveller_Rig_v01:Traveller_ROOT_Jnt/Traveller_Rig_v01:Traveller_Spine_01_Jnt");
        newRootBone.parent = playerModelClone.transform;
        Destroy(playerModelClone.transform.Find("Traveller_Rig_v01:Traveller_Trajectory_Jnt").gameObject);

        void SetUpArmModel(GameObject arm, Transform rootBone)
        {
            // destroys everything but the arm
            var oldParent = arm.transform.parent;
            arm.transform.parent = arm.transform.parent.parent;
            Destroy(oldParent.gameObject);

            arm.layer = 27;
            var renderer = arm.GetComponent<SkinnedMeshRenderer>();
            renderer.updateWhenOffscreen = true;

            // delete unneeded bones
            renderer.rootBone = rootBone;
            var newBones = new List<Transform>();
            foreach (Transform bone in renderer.bones)
            {
                if (bone != null)
                {
                    newBones.Add(bone);
                }
            }

            renderer.bones = newBones.ToArray();
        }

        SetUpArmModel(playerModelClone.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject, newRootBone);
        SetUpArmModel(playerModelClone.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject, newRootBone);

        // Move to transform
        playerModelClone.transform.parent = parent;
        playerModelClone.transform.localPosition = Vector3.zero;
        playerModelClone.transform.localRotation = Quaternion.identity;
        newRootBone.localPosition = new Vector3(-0.2741f, -6.3146f, -0.6957f);

        var viewmodelArm = playerModelClone.AddComponent<ViewmodelArm>();
        viewmodelArm._viewmodelArmNoSuit = playerModelClone.transform.Find("player_mesh_noSuit:Player_RightArm").gameObject;
        viewmodelArm._viewmodelArmSuit = playerModelClone.transform.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        viewmodelArm._bones = playerModelClone.GetComponentInChildren<SkinnedMeshRenderer>().bones;

        return viewmodelArm;
    }

    private void SetArmShader(ArmShader armShader)
    {
        // get shaders if we don't have them
        s_armShaders ??=
        [
            Shader.Find("Standard"),
            Shader.Find("Outer Wilds/Utility/View Model"),
            Shader.Find("Outer Wilds/Utility/View Model (Cutoff)")
        ];

        // apply shaders to materials
        var noSuitMesh = _viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>();
        noSuitMesh.materials[0].shader = s_armShaders[(int)armShader];
        noSuitMesh.materials[1].shader = s_armShaders[(int)armShader];
        _viewmodelArmSuit.GetComponent<SkinnedMeshRenderer>().material.shader = s_armShaders[(int)armShader];
    }

    private void OutputBoneTransforms()
    {
        foreach (var bone in _bones)
        {
            ModMain.Instance.ModHelper.Console.WriteLine($"{bone.name}: localPosition = {bone.localPosition}, localRotation = {bone.localRotation}");
        }
    }

    private void Awake()
    {

        Transform playerTransform = Locator.GetPlayerController().transform;
        _playerRightArmNoSuit = playerTransform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerRightArmSuit = playerTransform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
    }

    private void LateUpdate()
    {
        if (!ModMain.Instance.IsViewModelHandsEnabled)
        {
            gameObject.SetActive(false);
            return;
        }

        _viewmodelArmNoSuit.SetActive(_playerRightArmNoSuit.activeInHierarchy);
        _viewmodelArmSuit.SetActive(_playerRightArmSuit.activeInHierarchy);
    }
}
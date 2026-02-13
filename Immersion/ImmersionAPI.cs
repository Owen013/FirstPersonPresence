using Immersion.Components;
using Immersion.Objects;
using System;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public bool AreViewmodelArmsEnabled()
    {
        return Config.EnableViewmodelArms;
    }
    
    public void LoadArmData(string jsonPath)
    {
        ArmData.LoadArmData(jsonPath);
    }

    public GameObject CreateViewmodelArm(PlayerTool tool)
    {
        return ViewmodelArm.NewViewmodelArm(tool).gameObject;
    }

    public GameObject CreateViewmodelArm(OWItem item)
    {
        return ViewmodelArm.NewViewmodelArm(item).gameObject;
    }

    public void SetArmData(GameObject viewmodelArmObject, string itemName)
    {
        viewmodelArmObject.GetComponent<ViewmodelArm>()?.SetArmData(itemName);
    }

    [Obsolete("Immersion no longer changes AnimSpeed.")]
    public float GetAnimSpeed()
    {
        return 1f;
    }

    [Obsolete("No longer works. Use CreateViewmodelArm() instead.")]
    public GameObject NewViewmodelArm(PlayerTool playerTool, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
    {
        return null; // ViewmodelArm.NewViewmodelArm(playerTool, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
    }

    [Obsolete("No longer works. Use CreateViewmodelArm() instead.")]
    public GameObject NewViewmodelArm(OWItem owItem, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
    {
        return null; // ViewmodelArm.NewViewmodelArm(owItem, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
    }
}
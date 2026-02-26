using Immersion.Components;
using Immersion.Objects;
using System;
using UnityEngine;

namespace Immersion.APIs;

public class ImmersionAPI
{
    public bool AreViewmodelArmsEnabled()
    {
        return Config.EnableViewmodelArms;
    }

    public GameObject CreateViewmodelArm(PlayerTool tool)
    {
        return ViewmodelArm.NewViewmodelArm(tool).gameObject;
    }

    public GameObject CreateViewmodelArm(OWItem item)
    {
        return ViewmodelArm.NewViewmodelArm(item).gameObject;
    }

    public void LoadCustomArmData(string jsonPath)
    {
        ArmData.LoadCustomArmData(jsonPath);
    }

    public void SetArmData(GameObject viewmodelArmObject, string armDataID)
    {
        viewmodelArmObject.GetComponent<ViewmodelArm>()?.SetArmData(armDataID);
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
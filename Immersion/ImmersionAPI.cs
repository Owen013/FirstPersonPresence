using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public float GetAnimSpeed()
    {
        if (AnimSpeedController.Instance == null) return 1f;
        return AnimSpeedController.Instance.AnimSpeed;
    }

    public GameObject NewViewmodelArm(PlayerTool playerTool, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
    {
        return ViewmodelArm.NewViewmodelArm(playerTool, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
    }

    public GameObject NewViewmodelArm(OWItem owItem, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
    {
        return ViewmodelArm.NewViewmodelArm(owItem, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
    }
}
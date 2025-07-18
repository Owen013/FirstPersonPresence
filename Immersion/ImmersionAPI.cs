using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public float GetAnimSpeed()
    {
        return AnimSpeedController.Instance.AnimSpeed;
    }

    public GameObject NewViewmodelArm(PlayerTool playerTool, (Vector3 position, Quaternion rotation, float scale) armTransform, int armShader = 0)
    {
        return ViewmodelArm.NewViewmodelArm(playerTool, armTransform, (ViewmodelArm.ArmShader)armShader).gameObject;
    }

    public GameObject NewViewmodelArm(OWItem owItem, (Vector3 position, Quaternion rotation, float scale) armTransform, int armShader = 0)
    {
        return ViewmodelArm.NewViewmodelArm(owItem, armTransform, (ViewmodelArm.ArmShader)armShader).gameObject;
    }
}
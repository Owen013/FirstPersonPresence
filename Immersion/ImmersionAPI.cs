using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public float GetAnimSpeed()
    {
        return AnimSpeedController.Instance.AnimSpeed;
    }

    public GameObject NewViewmodelArm(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false)
    {
        return ViewmodelArmHandler.NewViewmodelArm(parent, localPos, localRot, scale, useDefaultShader).gameObject;
    }
}
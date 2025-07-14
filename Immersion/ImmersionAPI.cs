using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public float GetAnimSpeed()
    {
        return AnimSpeedController.Instance.AnimSpeed;
    }

    public static ViewmodelArm NewViewmodelArm(Transform parent, (Vector3 position, Quaternion rotation, float scale) armTransform, ViewmodelArm.ArmShader shader, OWItem owItem = null, bool replace = false)
    {
        return ViewmodelArm.NewViewmodelArm(parent, armTransform, shader, owItem, replace);
    }
}
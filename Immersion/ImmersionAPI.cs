using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public enum ArmShader
    {
        Default,
        Viewmodel,
        ViewmodelCutoff
    }

    public float GetAnimSpeed()
    {
        return AnimSpeedController.Instance.AnimSpeed;
    }

    public static GameObject NewViewmodelArm(Transform parent, (Vector3 position, Quaternion rotation, float scale) armTransform, ArmShader shader, OWItem owItem = null)
    {
        return ViewmodelArm.NewViewmodelArm(parent, armTransform, (ViewmodelArm.ArmShader)shader, owItem).gameObject;
    }
}
using Immersion.Components;
using UnityEngine;

namespace Immersion.Interfaces;

public interface IImmersion
{
    /// <returns>Animation speed modifier set by Immersion</returns>
    public float GetAnimSpeed();

    /// <param name="shader">The shader used by the Viewmodel Arm (0 = Standard, 1 = Viewmodel, 2 = ViewmodelCutoff).</param>
    public GameObject NewViewmodelArm(PlayerTool playerTool, (Vector3 position, Quaternion rotation, float scale) armTransform, int armShader = 0);

    /// <param name="shader">The shader used by the Viewmodel Arm (0 = Standard, 1 = Viewmodel, 2 = ViewmodelCutoff).</param>
    public GameObject NewViewmodelArm(OWItem owItem, (Vector3 position, Quaternion rotation, float scale) armTransform, int armShader = 0);
}
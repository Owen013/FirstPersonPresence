using UnityEngine;

namespace Immersion.Interfaces;

public interface IImmersion
{
    /// <summary>
    /// Gets the animation speed multiplier from Immersion
    /// </summary>
    /// <returns>The animation speed multiplier</returns>
    public float GetAnimSpeed();

    /// <summary>
    /// Creates a new ViewmodelArm on a PlayerTool
    /// </summary>
    /// <param name="playerTool">The tool that this ViewmodelArm is attached to</param>
    /// <param name="localPos">Local position of ViewmodelArm</param>
    /// <param name="localRot">Local rotation of ViewmodelArm</param>
    /// <param name="localScale">Local scale of ViewmodelArm</param>
    /// <param name="armShader">The shader used by the ViewmodelArm. 0 = Standard, 1 = Viewmodel, 2 = Viewmodel (Cutoff)</param>
    /// <returns></returns>
    public GameObject NewViewmodelArm(PlayerTool playerTool, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0);

    /// <summary>
    /// Creates a new ViewmodelArm on an OWItem
    /// </summary>
    /// <param name="playerTool">The item that this ViewmodelArm is attached to</param>
    /// <param name="localPos">Local position of ViewmodelArm</param>
    /// <param name="localRot">Local rotation of ViewmodelArm</param>
    /// <param name="localScale">Local scale of ViewmodelArm</param>
    /// <param name="armShader">The shader used by the ViewmodelArm. 0 = Standard, 1 = Viewmodel, 2 = Viewmodel (Cutoff)</param>
    /// <returns></returns>
    public GameObject NewViewmodelArm(OWItem owItem, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0);
}
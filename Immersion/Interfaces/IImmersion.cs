using UnityEngine;

namespace Immersion.Interfaces;

public interface IImmersion
{
    /// <summary>
    /// Whether or not the Viewmodel Arms feature is enabled
    /// </summary>
    /// <returns>true if Viewmodel Arms are enabled, false if not</returns>
    public bool AreViewmodelArmsEnabled();

    /// <summary>
    /// Loads ArmData from JSON.
    /// </summary>
    /// <param name="jsonPath">The path to the JSON containing the custom ArmData information</param>
    public void LoadArmData(string jsonPath);

    /// <summary>
    /// Creates a Viewmodel Arm on a PlayerTool
    /// </summary>
    /// <param name="tool">The PlayerTool to add a Viewmodel Arm to</param>
    /// <returns>The Viewmodel Arm's GameObject</returns>
    public GameObject CreateViewmodelArm(PlayerTool tool);

    /// <summary>
    /// Creates a Viewmodel Arm on an OWItem
    /// </summary>
    /// <param name="item">The OWItem to add a Viewmodel Arm to</param>
    /// <returns>The Viewmodel Arm's GameObject</returns>
    public GameObject CreateViewmodelArm(OWItem item);

    /// <summary>
    /// Sets the ArmData for a Viewmodel Arm
    /// </summary>
    /// <param name="viewmodelArmObject">The GameObject of the ViewmodelArm</param>
    /// <param name="itemName">The name (from the JSON) of the Arm Data information</param>
    public void SetArmData(GameObject viewmodelArmObject, string itemName);
}
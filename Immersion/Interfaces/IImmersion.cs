namespace Immersion.Interfaces;

public interface IImmersion
{
    /// <summary>
    /// Gets the animation speed multiplier from Immersion
    /// </summary>
    /// <returns>The animation speed multiplier</returns>
    public float GetAnimSpeed();

    /// <summary>
    /// Loads custom ArmData from a JSON at a specified path
    /// </summary>
    /// <param name="jsonPath">The path of the custom ArmData JSON</param>
    public void LoadArmData(string jsonPath);

    /// <summary>
    /// Creates a new Viewmodel Arm for the specified tool
    /// </summary>
    /// <param name="tool">The tool to add a Viewmodel Arm to</param>
    /// <param name="itemName">The name tied to the ArmData this arm should use</param>
    public void CreateViewmodelArm(PlayerTool tool, string itemName);

    /// <summary>
    /// Creates a new Viewmodel Arm for the specified item
    /// </summary>
    /// <param name="item">The item to add a Viewmodel Arm to</param>
    /// <param name="itemName">The name tied to the ArmData this arm should use</param>
    public void CreateViewmodelArm(OWItem item, string itemName);
}
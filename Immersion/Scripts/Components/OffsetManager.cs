using UnityEngine;

namespace Immersion.Scripts.Components;

public class OffsetManager : MonoBehaviour
{
    public static OffsetManager Instance { get; private set; }

    public OffsetRoot CameraOffsetRoot { get; private set; }

    public OffsetRoot ItemToolOffsetRoot { get; private set; }

    public OffsetRoot SignalscopeOffsetRoot { get; private set; }

    public OffsetRoot ProbeLauncherOffsetRoot { get; private set; }

    public OffsetRoot TranslatorOffsetRoot { get; private set; }

    /// <summary>
    /// Applies a translational offset to the player camera.
    /// </summary>
    /// <param name="position">The local position of the offset.</param>
    public void AddCameraOffset(Vector3 position) =>
        CameraOffsetRoot.AddOffset(position);

    /// <summary>
    /// Applies a rotational offset to the player camera.
    /// </summary>
    /// <param name="rotation">The local rotation of the offset.</param>
    public void AddCameraOffset(Quaternion rotation) =>
        CameraOffsetRoot.AddOffset(rotation);

    /// <summary>
    /// Applies a translational and rotational offset to the player camera.
    /// </summary>
    /// <param name="position">The local position of the offset.</param>
    /// <param name="rotation">The local rotation of the offset.</param>
    public void AddCameraOffset(Vector3 position, Quaternion rotation)
    {
        AddCameraOffset(position);
        AddCameraOffset(rotation);
    }

    /// <summary>
    /// Applies a translational offset to all tools that is scaled to match the tool scale.
    /// </summary>
    /// <param name="position">The local position of the offset.</param>
    public void AddToolOffsets(Vector3 position)
    {
        // apply different scaling factors for different tools
        ItemToolOffsetRoot.AddOffset(position);
        SignalscopeOffsetRoot.AddOffset(position);
        ProbeLauncherOffsetRoot.AddOffset(3f * position);
        TranslatorOffsetRoot.AddOffset(3f * position);
    }

    /// <summary>
    /// Applies a rotational offset to all tools.
    /// </summary>
    /// <param name="rotation">The local rotation of the offset.</param>
    public void AddToolOffsets(Quaternion rotation)
    {
        // apply same rotation offset for all tools
        ItemToolOffsetRoot.AddOffset(rotation);
        SignalscopeOffsetRoot.AddOffset(rotation);
        ProbeLauncherOffsetRoot.AddOffset(rotation);
        TranslatorOffsetRoot.AddOffset(rotation);
    }

    /// <summary>
    /// Applies a translational offset (rescaling it so that it looks the same on all tools) and a rotational offset to all tools.
    /// </summary>
    /// <param name="position">The local position of the offset.</param>
    /// <param name="rotation">The local rotation of the offset.</param>
    public void AddToolOffsets(Vector3 position, Quaternion rotation)
    {
        AddToolOffsets(position);
        AddToolOffsets(rotation);
    }

    private void Awake()
    {
        Instance = this;

        // create offset roots
        CameraOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Camera", Locator.GetPlayerCamera().gameObject);
        var toolModeSwapper = Locator.GetToolModeSwapper();
        ItemToolOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ItemCarryTool", toolModeSwapper.GetItemCarryTool().gameObject);
        SignalscopeOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Signalscope", toolModeSwapper.GetSignalScope().gameObject);
        ProbeLauncherOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ProbeLauncher", toolModeSwapper.GetProbeLauncher().gameObject);
        TranslatorOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_NomaiTranslatorProp", toolModeSwapper.GetTranslator().gameObject);

        gameObject.AddComponent<ViewbobController>();
        gameObject.AddComponent<ViewmodelOffsetController>();
        gameObject.AddComponent<ViewmodelSwayController>();
        gameObject.AddComponent<BreathingAnimController>();
        gameObject.AddComponent<ScoutAnimController>();
        gameObject.AddComponent<LandingAnimController>();
        gameObject.AddComponent<HideStowedItemsController>();

        if (ModMain.HikersModAPI != null)
            gameObject.AddComponent<SprintingAnimController>();
    }
}
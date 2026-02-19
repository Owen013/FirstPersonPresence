using UnityEngine;

namespace Immersion.Components;

public class OffsetManager : MonoBehaviour
{
    public static OffsetManager Instance { get; private set; }

    public OffsetRoot CameraOffsetRoot { get; private set; }

    public OffsetRoot ItemToolOffsetRoot { get; private set; }

    public OffsetRoot SignalscopeOffsetRoot { get; private set; }

    public OffsetRoot ProbeLauncherOffsetRoot { get; private set; }

    public OffsetRoot TranslatorOffsetRoot { get; private set; }

    public void AddCameraOffset(Vector3 position) =>
        CameraOffsetRoot.AddOffset(position);

    public void AddCameraOffset(Quaternion rotation) =>
        CameraOffsetRoot.AddOffset(rotation);

    public void AddCameraOffset(Vector3 position, Quaternion rotation)
    {
        AddCameraOffset(position);
        AddCameraOffset(rotation);
    }

    public void AddToolOffsets(Vector3 position)
    {
        // apply different scaling factors for different tools
        ItemToolOffsetRoot.AddOffset(position);
        SignalscopeOffsetRoot.AddOffset(position);
        ProbeLauncherOffsetRoot.AddOffset(3f * position);
        TranslatorOffsetRoot.AddOffset(3f * position);
    }

    public void AddToolOffsets(Quaternion rotation)
    {
        // apply same rotation offset for all tools
        ItemToolOffsetRoot.AddOffset(rotation);
        SignalscopeOffsetRoot.AddOffset(rotation);
        ProbeLauncherOffsetRoot.AddOffset(rotation);
        TranslatorOffsetRoot.AddOffset(rotation);
    }

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
        gameObject.AddComponent<HandHeightOffsetController>();
        gameObject.AddComponent<HandSwayController>();
        gameObject.AddComponent<BreathingAnimController>();
        gameObject.AddComponent<ScoutAnimController>();
        gameObject.AddComponent<LandingAnimController>();
        gameObject.AddComponent<HideStowedItemsController>();

        if (ModMain.HikersModAPI != null)
            gameObject.AddComponent<SprintingAnimController>();
    }
}
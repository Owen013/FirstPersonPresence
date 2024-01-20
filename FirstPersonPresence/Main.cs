using HarmonyLib;
using FirstPersonPresence.APIs;
using FirstPersonPresence.Components;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace FirstPersonPresence;

public class Main : ModBehaviour
{
    public static Main Instance;
    public ISmolHatchling SmolHatchlingAPI;

    // Config
    public float ViewBobXSensitivity;
    public float ViewBobYSensitivity;
    public float ToolBobSensitivity;
    public float ToolHeightYSensitivity;
    public float ToolHeightZSensitivity;
    public bool DebugLogEnabled;

    public override object GetApi()
    {
        return new FirstPersonPresenceAPI();
    }

    public override void Configure(IModConfig config)
    {
        ViewBobXSensitivity = config.GetSettingsValue<float>("View Bob X Sensitivity");
        ViewBobYSensitivity = config.GetSettingsValue<float>("View Bob Y Sensitivity");
        ToolBobSensitivity = config.GetSettingsValue<float>("Tool Bob Sensitivity");
        ToolHeightYSensitivity = config.GetSettingsValue<float>("Dynamic Tool Height Y Sensitivity");
        ToolHeightZSensitivity = config.GetSettingsValue<float>("Dynamic Tool Height Z Sensitivity");
        DebugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");
    }

    public void DebugLog(string text, MessageType type = MessageType.Message, bool forceMessage = false)
    {
        if (!DebugLogEnabled && !forceMessage) return;
        ModHelper.Console.WriteLine(text, type);
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(typeof(Main));
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        DebugLog($"First Person Presence is ready to go!", MessageType.Success, true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void OnCameraStart(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraController>();
    }
}
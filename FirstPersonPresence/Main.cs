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
    public float ViewBobXAmount;
    public float ViewBobYAmount;
    public float ToolBobAmount;
    public float ToolHeightYAmount;
    public float ToolHeightZAmount;
    public bool IsDebugLogEnabled;

    public override object GetApi()
    {
        return new FirstPersonPresenceAPI();
    }

    public override void Configure(IModConfig config)
    {
        ViewBobXAmount = config.GetSettingsValue<float>("View Bob X Amount");
        ViewBobYAmount = config.GetSettingsValue<float>("View Bob Y Amount");
        ToolBobAmount = config.GetSettingsValue<float>("Tool Bob Amount");
        ToolHeightYAmount = config.GetSettingsValue<float>("Dynamic Tool Height Y Amount");
        ToolHeightZAmount = config.GetSettingsValue<float>("Dynamic Tool Height Z Amount");
        IsDebugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");
    }

    public void DebugLog(string text, MessageType type = MessageType.Message, bool forceMessage = false)
    {
        if (!IsDebugLogEnabled && !forceMessage) return;
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
using HarmonyLib;
using FirstPersonPresence.APIs;
using FirstPersonPresence.Components;
using OWML.Common;
using OWML.ModHelper;

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
    public float ToolSwaySensitivity;
    public float ToolSwaySmoothing;
    public bool IsDebugLogEnabled;

    public override object GetApi()
    {
        return new FirstPersonPresenceAPI();
    }

    public override void Configure(IModConfig config)
    {
        ViewBobXAmount = config.GetSettingsValue<float>("ViewBobX");
        ViewBobYAmount = config.GetSettingsValue<float>("ViewBobY");
        ToolBobAmount = config.GetSettingsValue<float>("ToolBob");
        ToolHeightYAmount = config.GetSettingsValue<float>("ToolHeightY");
        ToolHeightZAmount = config.GetSettingsValue<float>("ToolHeightZ");
        ToolSwaySensitivity = config.GetSettingsValue<float>("ToolSway");
        ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");
        IsDebugLogEnabled = config.GetSettingsValue<bool>("DebugLog");
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
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Awake))]
    private static void OnCameraAwake(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<RootController>();
    }

    //[HarmonyPostfix]
    //[HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Awake))]
    //private static void OnAnimatorAwake(PlayerAnimController __instance)
    //{
    //    __instance.gameObject.AddComponent<StepCounter>();
    //}
}
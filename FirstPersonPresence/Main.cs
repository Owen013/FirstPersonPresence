using HarmonyLib;
using FirstPersonPresence.APIs;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace FirstPersonPresence;

public class Main : ModBehaviour
{
    public static Main Instance;
    public ISmolHatchling SmolHatchlingAPI;

    public override object GetApi()
    {
        return new FirstPersonPresenceAPI();
    }

    public override void Configure(IModConfig config)
    {
        Config.ViewBobXAmount = config.GetSettingsValue<float>("ViewBobX");
        Config.ViewBobYAmount = config.GetSettingsValue<float>("ViewBobY");
        Config.ToolBobAmount = config.GetSettingsValue<float>("ToolBob");
        Config.ToolHeightYAmount = config.GetSettingsValue<float>("ToolHeightY");
        Config.ToolHeightZAmount = config.GetSettingsValue<float>("ToolHeightZ");
        Config.ToolSwaySensitivity = config.GetSettingsValue<float>("ToolSway");
        Config.ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");
        Config.IsDebugLogEnabled = config.GetSettingsValue<bool>("DebugLog");
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        Log($"First Person Presence is ready to go!", MessageType.Success);
    }

    public void Log(string text, MessageType type = MessageType.Message)
    {
        ModHelper.Console.WriteLine(text, type);
    }
}
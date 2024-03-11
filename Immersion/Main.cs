using HarmonyLib;
using Immersion.APIs;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion;

public class Main : ModBehaviour
{
    public static Main Instance;
    public ISmolHatchling SmolHatchlingAPI;

    public override object GetApi()
    {
        return new ImmersionAPI();
    }

    public override void Configure(IModConfig config)
    {
        if (config.GetSettingsValue<bool>("EnableViewBob") == false)
        {
            Config.ViewBobXAmount = 0f;
            Config.ViewBobYAmount = 0f;
            Config.ViewBobRollAmount = 0f;
            Config.ViewBobPitchAmount = 0f;
        }
        else
        {
            Config.ViewBobXAmount = config.GetSettingsValue<float>("ViewBobX");
            Config.ViewBobYAmount = config.GetSettingsValue<float>("ViewBobY");
            Config.ViewBobRollAmount = config.GetSettingsValue<float>("ViewBobRoll");
            Config.ViewBobPitchAmount = config.GetSettingsValue<float>("ViewBobPitch");

            if (config.GetSettingsValue<bool>("FlipViewBob")) Config.ViewBobYAmount *= -1f;
        }

        if (config.GetSettingsValue<bool>("EnableToolBob") == false)
        {
            Config.ToolBobXAmount = 0f;
            Config.ToolBobYAmount = 0f;
            Config.ToolBobZAmount = 0f;
            Config.ToolBobRollAmount = 0f;
            Config.ToolBobPitchAmount = 0f;
        }
        else
        {
            Config.ToolBobXAmount = config.GetSettingsValue<float>("ToolBobX");
            Config.ToolBobYAmount = config.GetSettingsValue<float>("ToolBobY");
            Config.ToolBobZAmount = config.GetSettingsValue<float>("ToolBobZ");
            Config.ToolBobRollAmount = config.GetSettingsValue<float>("ToolBobRoll");
            Config.ToolBobPitchAmount = config.GetSettingsValue<float>("ToolBobPitch");

            if (config.GetSettingsValue<bool>("FlipToolBob"))
            {
                Config.ToolBobYAmount *= -1f;
                Config.ToolBobPitchAmount *= -1f;
            }
        }

        Config.UseJumpAnim = config.GetSettingsValue<bool>("UseJumpAnim");
        Config.UseFallAnim = config.GetSettingsValue<bool>("UseFallAnim");
        Config.UseLandingAnim = config.GetSettingsValue<bool>("UseLandingAnim");
        Config.UseScoutAnim = config.GetSettingsValue<bool>("UseScoutAnim");

        if (config.GetSettingsValue<bool>("EnableToolHeight") == false)
        {
            Config.ToolHeightYAmount = 0f;
            Config.ToolHeightZAmount = 0f;
        }
        else
        {
            Config.ToolHeightYAmount = config.GetSettingsValue<float>("ToolHeightY");
            Config.ToolHeightZAmount = config.GetSettingsValue<float>("ToolHeightZ");
        }
        if (config.GetSettingsValue<bool>("EnableToolSway") == false)
        {
            Config.ToolSwaySensitivity = 0f;
            Config.ToolSwaySmoothing = 0f;
            Config.MaxSwayX = 0f;
            Config.MaxSwayY = 0f;
        }
        else
        {
            Config.ToolSwaySensitivity = config.GetSettingsValue<float>("ToolSwaySensitivity");
            Config.ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");
            Config.MaxSwayX = config.GetSettingsValue<float>("MaxSwayX");
            Config.MaxSwayY = config.GetSettingsValue<float>("MaxSwayY");
        }
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        Log($"Immersion is ready to go!", MessageType.Success);
    }

    public void Log(string text, MessageType type = MessageType.Message)
    {
        ModHelper.Console.WriteLine(text, type);
    }
}
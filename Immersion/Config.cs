using OWML.Common;

namespace Immersion;

public static class Config
{
    public delegate void ConfigureEvent();
    public static event ConfigureEvent OnConfigure;

    public static bool IsViewModelHandsEnabled { get; private set; }
    public static float ViewBobXAmount { get; private set; }
    public static float ViewBobRollAmount { get; private set; }
    public static float ViewBobPitchAmount { get; private set; }
    public static float ViewBobYAmount { get; private set; }
    public static float ToolBobXAmount { get; private set; }
    public static float ToolBobYAmount { get; private set; }
    public static float ToolBobZAmount { get; private set; }
    public static float ToolBobRollAmount { get; private set; }
    public static float ToolBobPitchAmount { get; private set; }
    public static float ToolSwaySensitivity { get; private set; }
    public static float ToolSwaySmoothing { get; private set; }
    public static string ToolHeightBehavior { get; private set; }
    public static float ToolHeightYAmount { get; private set; }
    public static float ToolHeightZAmount { get; private set; }
    public static bool IsJumpAnimEnabled { get; private set; }
    public static bool IsFallAnimEnabled { get; private set; }
    public static bool IsLandingAnimEnabled { get; private set; }
    public static bool IsScoutAnimEnabled { get; private set; }

    public static bool IsHideStowedItemsEnabled { get; private set; }
    public static bool IsLeftyModeEnabled { get; private set; }

    public static void UpdateConfig(IModConfig config)
    {
        IsViewModelHandsEnabled = config.GetSettingsValue<bool>("EnableViewmodelHands");

        if (config.GetSettingsValue<bool>("EnableViewBob") == false)
        {
            ViewBobXAmount = 0f;
            ViewBobYAmount = 0f;
            ViewBobRollAmount = 0f;
            ViewBobPitchAmount = 0f;
        }
        else
        {
            ViewBobXAmount = config.GetSettingsValue<float>("ViewBobX");
            ViewBobYAmount = config.GetSettingsValue<float>("ViewBobY");
            ViewBobRollAmount = config.GetSettingsValue<float>("ViewBobRoll");
            ViewBobPitchAmount = config.GetSettingsValue<float>("ViewBobPitch");

            if (config.GetSettingsValue<bool>("FlipViewBob")) ViewBobYAmount *= -1f;
        }

        if (config.GetSettingsValue<bool>("EnableToolBob") == false)
        {
            ToolBobXAmount = 0f;
            ToolBobYAmount = 0f;
            ToolBobZAmount = 0f;
            ToolBobRollAmount = 0f;
            ToolBobPitchAmount = 0f;
        }
        else
        {
            ToolBobXAmount = config.GetSettingsValue<float>("ToolBobX");
            ToolBobYAmount = config.GetSettingsValue<float>("ToolBobY");
            ToolBobZAmount = config.GetSettingsValue<float>("ToolBobZ");
            ToolBobRollAmount = config.GetSettingsValue<float>("ToolBobRoll");
            ToolBobPitchAmount = config.GetSettingsValue<float>("ToolBobPitch");

            if (config.GetSettingsValue<bool>("FlipToolBob"))
            {
                ToolBobYAmount *= -1f;
                ToolBobPitchAmount *= -1f;
            }
        }

        if (config.GetSettingsValue<bool>("EnableToolSway") == false)
        {
            ToolSwaySensitivity = 0f;
            ToolSwaySmoothing = 0f;
        }
        else
        {
            ToolSwaySensitivity = config.GetSettingsValue<float>("ToolSwaySensitivity");
            ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");
        }

        ToolHeightBehavior = config.GetSettingsValue<string>("ToolHeightBehavior");
        if (config.GetSettingsValue<bool>("EnableToolHeight") == false)
        {
            ToolHeightYAmount = 0f;
            ToolHeightZAmount = 0f;
        }
        else
        {
            ToolHeightYAmount = config.GetSettingsValue<float>("ToolHeightY");
            ToolHeightZAmount = config.GetSettingsValue<float>("ToolHeightZ");
        }

        IsJumpAnimEnabled = config.GetSettingsValue<bool>("UseJumpAnim");
        IsFallAnimEnabled = config.GetSettingsValue<bool>("UseFallAnim");
        IsLandingAnimEnabled = config.GetSettingsValue<bool>("UseLandingAnim");
        IsScoutAnimEnabled = config.GetSettingsValue<bool>("UseScoutAnim");
        IsHideStowedItemsEnabled = config.GetSettingsValue<bool>("HideStowedItems");
        IsLeftyModeEnabled = config.GetSettingsValue<bool>("UseLeftyMode");

        OnConfigure?.Invoke();
    }
}
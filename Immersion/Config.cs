using OWML.Common;

namespace Immersion;

public static class Config
{
    public static bool IsViewModelHandsEnabled { get; private set; }

    public static bool IsViewBobEnabled { get; private set; }

    public static float ViewBobXAmount { get; private set; }

    public static float ViewBobRollAmount { get; private set; }

    public static float ViewBobPitchAmount { get; private set; }

    public static float ViewBobYAmount { get; private set; }

    public static bool IsToolBobEnabled { get; private set; }

    public static float ToolBobXAmount { get; private set; }

    public static float ToolBobYAmount { get; private set; }

    public static float ToolBobZAmount { get; private set; }

    public static float ToolBobRollAmount { get; private set; }

    public static float ToolBobPitchAmount { get; private set; }

    public static bool IsToolSwayEnabled { get; private set; }

    public static float ToolSwayTranslateAmount { get; private set; }

    public static float ToolSwayRotateAmount { get; private set; }

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

    public delegate void ConfigureEvent();

    public static event ConfigureEvent OnConfigure;

    public static void UpdateConfig(IModConfig config)
    {
        IsViewModelHandsEnabled = config.GetSettingsValue<bool>("EnableViewmodelHands");

        IsViewBobEnabled = config.GetSettingsValue<bool>("EnableViewBob");
        ViewBobXAmount = config.GetSettingsValue<float>("ViewBobX");
        ViewBobYAmount = config.GetSettingsValue<float>("ViewBobY");
        ViewBobRollAmount = config.GetSettingsValue<float>("ViewBobRoll");
        ViewBobPitchAmount = config.GetSettingsValue<float>("ViewBobPitch");

        IsToolBobEnabled = config.GetSettingsValue<bool>("EnableToolBob");
        ToolBobXAmount = config.GetSettingsValue<float>("ToolBobX");
        ToolBobYAmount = config.GetSettingsValue<float>("ToolBobY");
        ToolBobZAmount = config.GetSettingsValue<float>("ToolBobZ");
        ToolBobRollAmount = config.GetSettingsValue<float>("ToolBobRoll");
        ToolBobPitchAmount = config.GetSettingsValue<float>("ToolBobPitch");

        IsToolSwayEnabled = config.GetSettingsValue<bool>("EnableToolSway");
        ToolSwayTranslateAmount = config.GetSettingsValue<float>("ToolSwayTranslateAmount");
        ToolSwayRotateAmount = config.GetSettingsValue<float>("ToolSwayRotateAmount");
        ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");

        ToolHeightBehavior = config.GetSettingsValue<string>("ToolHeightBehavior");
        ToolHeightYAmount = config.GetSettingsValue<float>("ToolHeightY");
        ToolHeightZAmount = config.GetSettingsValue<float>("ToolHeightZ");

        IsJumpAnimEnabled = config.GetSettingsValue<bool>("UseJumpAnim");
        IsFallAnimEnabled = config.GetSettingsValue<bool>("UseFallAnim");
        IsLandingAnimEnabled = config.GetSettingsValue<bool>("UseLandingAnim");
        IsScoutAnimEnabled = config.GetSettingsValue<bool>("UseScoutAnim");
        IsHideStowedItemsEnabled = config.GetSettingsValue<bool>("HideStowedItems");
        IsLeftyModeEnabled = config.GetSettingsValue<bool>("UseLeftyMode");

        OnConfigure?.Invoke();
    }
}
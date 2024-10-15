using OWML.Common;

namespace Immersion;

public static class Config
{
    public static bool IsViewModelHandsEnabled { get; private set; }

    public static bool IsCameraBobEnabled { get; private set; }

    public static float CameraBobXAmount { get; private set; }

    public static float CameraBobRollAmount { get; private set; }

    public static float CameraBobPitchAmount { get; private set; }

    public static float CameraBobYAmount { get; private set; }

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

    public static string DynamicToolPosBehavior { get; private set; }

    public static float DynamicToolPosYAmount { get; private set; }

    public static float DynamicToolPosZAmount { get; private set; }

    public static bool IsScoutAnimEnabled { get; private set; }

    public static bool IsHideStowedItemsEnabled { get; private set; }

    public static bool IsLeftyModeEnabled { get; private set; }

    public delegate void ConfigureEvent();

    public static event ConfigureEvent OnConfigure;

    public static void UpdateConfig(IModConfig config)
    {
        IsViewModelHandsEnabled = config.GetSettingsValue<bool>("EnableViewmodelHands");

        IsCameraBobEnabled = config.GetSettingsValue<bool>("EnableViewBob");
        CameraBobXAmount = config.GetSettingsValue<float>("ViewBobXAmount");
        CameraBobYAmount = config.GetSettingsValue<float>("ViewBobYAmount");
        CameraBobRollAmount = config.GetSettingsValue<float>("ViewBobRollAmount");
        CameraBobPitchAmount = config.GetSettingsValue<float>("ViewBobPitchAmount");

        IsToolBobEnabled = config.GetSettingsValue<bool>("EnableToolBob");
        ToolBobXAmount = config.GetSettingsValue<float>("ToolBobXAmount");
        ToolBobYAmount = config.GetSettingsValue<float>("ToolBobYAmount");
        ToolBobZAmount = config.GetSettingsValue<float>("ToolBobZAmount");
        ToolBobRollAmount = config.GetSettingsValue<float>("ToolBobRollAmount");
        ToolBobPitchAmount = config.GetSettingsValue<float>("ToolBobPitchAmount");

        IsToolSwayEnabled = config.GetSettingsValue<bool>("EnableToolSway");
        ToolSwayTranslateAmount = config.GetSettingsValue<float>("ToolSwayTranslateAmount");
        ToolSwayRotateAmount = config.GetSettingsValue<float>("ToolSwayRotateAmount");
        ToolSwaySmoothing = config.GetSettingsValue<float>("ToolSwaySmoothing");

        DynamicToolPosBehavior = config.GetSettingsValue<string>("DynamicToolPosBehavior");
        DynamicToolPosYAmount = config.GetSettingsValue<float>("DynamicToolPosYAmount");
        DynamicToolPosZAmount = config.GetSettingsValue<float>("DynamicToolPosZAmount");

        IsScoutAnimEnabled = config.GetSettingsValue<bool>("UseScoutAnim");
        IsHideStowedItemsEnabled = config.GetSettingsValue<bool>("HideStowedItems");
        IsLeftyModeEnabled = config.GetSettingsValue<bool>("UseLeftyMode");

        OnConfigure?.Invoke();
    }
}
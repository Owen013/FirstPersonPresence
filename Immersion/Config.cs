using OWML.Common;

namespace Immersion;

public static class Config
{
    public static bool EnableViewmodelArms { get; private set; }

    public static bool EnableHeadBob { get; private set; }

    public static float HeadBobStrength { get; private set; }

    public static bool EnableHandBob { get; private set; }

    public static float HandBobStrength { get; private set; }

    public static bool EnableHandHeightOffset { get; private set; }

    public static float HandHeightOffsetStrength { get; private set; }

    public static bool EnableHandSway { get; private set; }

    public static float HandSwayStrength { get; private set; }

    public static bool EnableBreathingAnim { get; private set; }

    public static float BreathingAnimStrength { get; private set; }

    public static bool EnableScoutAnim { get; private set; }

    public static bool EnableLandingAnim { get; private set; }

    public static bool EnableSprintingAnim { get; private set; }

    public static bool FixItemClipping { get; private set; }

    public static bool HideStowedItems { get; private set; }

    public static void Configure(IModConfig config)
    {
        // viewmodel hands
        EnableViewmodelArms = config.GetSettingsValue<bool>("EnableViewmodelArms");

        // viewbob
        EnableHeadBob = config.GetSettingsValue<bool>("EnableHeadBob");
        HeadBobStrength = config.GetSettingsValue<float>("HeadBobStrength");
        EnableHandBob = config.GetSettingsValue<bool>("EnableHandBob");
        HandBobStrength = config.GetSettingsValue<float>("HandBobStrength");

        // dynamic tool pos
        EnableHandHeightOffset = config.GetSettingsValue<bool>("EnableHandHeightOffset");
        HandHeightOffsetStrength = config.GetSettingsValue<float>("HandHeightOffsetStrength");

        // tool sway
        EnableHandSway = config.GetSettingsValue<bool>("EnableHandSway");
        HandSwayStrength = config.GetSettingsValue<float>("HandSwayStrength");

        // breathing anim
        EnableBreathingAnim = config.GetSettingsValue<bool>("EnableBreathingAnim");
        BreathingAnimStrength = config.GetSettingsValue<float>("BreathingAnimStrength");

        // misc
        FixItemClipping = config.GetSettingsValue<bool>("FixHandClipping");
        EnableScoutAnim = config.GetSettingsValue<bool>("EnableScoutAnim");
        EnableLandingAnim = config.GetSettingsValue<bool>("EnableLandingAnim");
        EnableSprintingAnim = config.GetSettingsValue<bool>("EnableSprintingAnim");
        HideStowedItems = config.GetSettingsValue<bool>("HideStowedItems");
    }
}

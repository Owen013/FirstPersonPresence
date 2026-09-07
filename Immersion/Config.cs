using OWML.Common;
using System;

namespace Immersion;

static class Config
{
    public static bool EnableViewmodelArms { get; private set; }

    public static bool EnableHeadBob { get; private set; }

    public static float HeadBobScale { get; private set; }

    public static bool EnableViewmodelBob { get; private set; }

    public static float ViewmodelBobScale { get; private set; }

    public static bool EnableViewmodelOffset { get; private set; }

    public static float ViewmodelOffsetScale { get; private set; }

    public static bool EnableViewmodelSway { get; private set; }

    public static float ViewmodelSwayScale { get; private set; }

    public static bool EnableBreathingAnim { get; private set; }

    public static bool EnableCameraLandingAnim { get; private set; }

    public static bool EnableViewmodelLandingAnim { get; private set; }

    public static float MaxLandingAnimDistance { get; private set; }

    public static float MaxLandingAnimRecoverySpeed { get; private set; }

    public static float LandingAnimSmoothness { get; private set; }

    public static bool UseLandingCrouchAnim = false;

    public static float BreathingAnimScale { get; private set; }

    public static bool EnableScoutAnim { get; private set; }

    public static bool EnableSprintingAnim { get; private set; }

    public static bool FixViewmodelClipping { get; private set; }

    public static bool HideStowedItems { get; private set; }

    public static event Action OnConfigured;

    public static void Configure(IModConfig config)
    {
        // viewmodel hands
        EnableViewmodelArms = config.GetSettingsValue<bool>("EnableViewmodelArms");

        // viewbob
        EnableHeadBob = config.GetSettingsValue<bool>("EnableHeadBob");
        HeadBobScale = config.GetSettingsValue<float>("HeadBobScale");
        EnableViewmodelBob = config.GetSettingsValue<bool>("EnableViewmodelBob");
        ViewmodelBobScale = config.GetSettingsValue<float>("ViewmodelBobScale");

        // dynamic tool pos
        EnableViewmodelOffset = config.GetSettingsValue<bool>("EnableViewmodelOffset");
        ViewmodelOffsetScale = config.GetSettingsValue<float>("ViewmodelOffsetScale");

        // tool sway
        EnableViewmodelSway = config.GetSettingsValue<bool>("EnableViewmodelSway");
        ViewmodelSwayScale = config.GetSettingsValue<float>("ViewmodelSwayScale");

        // landing anim
        EnableCameraLandingAnim = config.GetSettingsValue<bool>("EnableCameraLandingAnim");
        EnableViewmodelLandingAnim = config.GetSettingsValue<bool>("EnableViewmodelLandingAnim");
        MaxLandingAnimDistance = config.GetSettingsValue<float>("MaxLandingAnimDistance");
        MaxLandingAnimRecoverySpeed = config.GetSettingsValue<float>("MaxLandingAnimRecoverySpeed");
        LandingAnimSmoothness = config.GetSettingsValue<float>("LandingAnimSmoothness");

        // breathing anim
        EnableBreathingAnim = config.GetSettingsValue<bool>("EnableBreathingAnim");
        BreathingAnimScale = config.GetSettingsValue<float>("BreathingAnimScale");

        // misc
        FixViewmodelClipping = config.GetSettingsValue<bool>("FixViewmodelClipping");
        EnableScoutAnim = config.GetSettingsValue<bool>("EnableScoutAnim");
        EnableSprintingAnim = config.GetSettingsValue<bool>("EnableSprintingAnim");
        HideStowedItems = config.GetSettingsValue<bool>("HideStowedItems");

        OnConfigured?.Invoke();
    }
}
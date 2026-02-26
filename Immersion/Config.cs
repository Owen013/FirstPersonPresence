using OWML.Common;
using System;

namespace Immersion;

public static class Config
{
    public static bool EnableViewmodelArms { get; private set; }

    public static bool EnableHeadBob { get; private set; }

    public static float HeadBobStrength { get; private set; }

    public static bool EnableViewmodelBob { get; private set; }

    public static float ViewmodelBobStrength { get; private set; }

    public static bool EnableViewmodelOffset { get; private set; }

    public static float ViewmodelOffsetStrength { get; private set; }

    public static bool EnableViewmodelSway { get; private set; }

    public static float ViewmodelSwayStrength { get; private set; }

    public static bool EnableBreathingAnim { get; private set; }

    public static bool EnableCameraLandingAnim { get; private set; }

    public static bool EnableViewmodelLandingAnim { get; private set; }

    public static float MaxLandingAnimDistance { get; private set; }

    public static float MaxLandingAnimRecoverySpeed { get; private set; }

    public static float LandingAnimRecoverySmoothness { get; private set; }

    public static bool UseLandingCrouchAnim = false;

    public static float BreathingAnimStrength { get; private set; }

    public static bool EnableScoutAnim { get; private set; }

    public static bool EnableSprintingAnim { get; private set; }

    public static bool FixViewmodelClipping { get; private set; }

    public static bool HideStowedItems { get; private set; }

    public static event Action OnConfigured;

    internal static void Configure(IModConfig config)
    {
        // viewmodel hands
        EnableViewmodelArms = config.GetSettingsValue<bool>("EnableViewmodelArms");

        // viewbob
        EnableHeadBob = config.GetSettingsValue<bool>("EnableHeadBob");
        HeadBobStrength = config.GetSettingsValue<float>("HeadBobStrength");
        EnableViewmodelBob = config.GetSettingsValue<bool>("EnableViewmodelBob");
        ViewmodelBobStrength = config.GetSettingsValue<float>("ViewmodelBobStrength");

        // dynamic tool pos
        EnableViewmodelOffset = config.GetSettingsValue<bool>("EnableViewmodelOffset");
        ViewmodelOffsetStrength = config.GetSettingsValue<float>("ViewmodelOffsetStrength");

        // tool sway
        EnableViewmodelSway = config.GetSettingsValue<bool>("EnableViewmodelSway");
        ViewmodelSwayStrength = config.GetSettingsValue<float>("ViewmodelSwayStrength");

        // landing anim
        EnableCameraLandingAnim = config.GetSettingsValue<bool>("EnableCameraLandingAnim");
        EnableViewmodelLandingAnim = config.GetSettingsValue<bool>("EnableViewmodelLandingAnim");
        MaxLandingAnimDistance = config.GetSettingsValue<float>("MaxLandingAnimDistance");
        MaxLandingAnimRecoverySpeed = config.GetSettingsValue<float>("MaxLandingAnimRecoverySpeed");
        LandingAnimRecoverySmoothness = config.GetSettingsValue<float>("LandingAnimRecoverySmoothness");

        // breathing anim
        EnableBreathingAnim = config.GetSettingsValue<bool>("EnableBreathingAnim");
        BreathingAnimStrength = config.GetSettingsValue<float>("BreathingAnimStrength");

        // misc
        FixViewmodelClipping = config.GetSettingsValue<bool>("FixViewmodelClipping");
        EnableScoutAnim = config.GetSettingsValue<bool>("EnableScoutAnim");
        EnableSprintingAnim = config.GetSettingsValue<bool>("EnableSprintingAnim");
        HideStowedItems = config.GetSettingsValue<bool>("HideStowedItems");

        OnConfigured?.Invoke();
    }
}
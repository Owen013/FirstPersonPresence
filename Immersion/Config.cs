using OWML.Common;
using System;

namespace Immersion
{
    public static class Config
    {
        public static bool EnableViewmodelArms { get; set; }

        public static bool EnableHeadBob { get; set; }

        public static float HeadBobScale { get; set; }

        public static bool EnableViewmodelBob { get; set; }

        public static float ViewmodelBobScale { get; set; }

        public static bool EnableViewmodelOffset { get; set; }

        public static float ViewmodelOffsetScale { get; set; }

        public static bool EnableViewmodelSway { get; set; }

        public static float ViewmodelSwayScale { get; set; }

        public static bool EnableBreathingAnim { get; set; }

        public static bool EnableCameraLandingAnim { get; set; }

        public static bool EnableViewmodelLandingAnim { get; set; }

        public static float MaxLandingAnimDistance { get; set; }

        public static float MaxLandingAnimRecoverySpeed { get; set; }

        public static float LandingAnimSmoothness { get; set; }

        public static bool UseLandingCrouchAnim = false;

        public static float BreathingAnimScale { get; set; }

        public static bool EnableScoutAnim { get; set; }

        public static bool EnableSprintingAnim { get; set; }

        public static bool FixViewmodelClipping { get; set; }

        public static bool HideStowedItems { get; set; }

        public static event Action OnConfigured;

        internal static void Configure(IModConfig config)
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
}
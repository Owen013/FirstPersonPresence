using HarmonyLib;
using Immersion.Components;
using Immersion.Interfaces;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public ISmolHatchling SmolHatchlingAPI { get; private set; }

    public IHikersMod HikersModAPI { get; private set; }

    public bool EnableViewmodelHands { get; private set; }

    public bool EnableHeadBob { get; private set; }

    public float HeadBobStrength { get; private set; }

    public bool EnableHandBob { get; private set; }

    public float HandBobStrength { get; private set; }

    public bool EnableHandHeightOffset { get; private set; }

    public float HandHeightOffsetStrength { get; private set; }

    public bool EnableHandSway { get; private set; }

    public float HandSwayStrength { get; private set; }

    public bool EnableBreathingAnim { get; private set; }

    public float BreathingAnimStrength { get; private set; }

    public bool EnableScoutAnim { get; private set; }

    public bool EnableLandingAnim { get; private set; }

    public bool EnableSprintingAnim { get; private set; }

	public bool FixItemClipping { get; private set; }

	public bool HideStowedItems { get; private set; }

	public override object GetApi()
    {
        // provide API for use by other mods
        return new ImmersionAPI();
    }

    public override void Configure(IModConfig config)
    {
        // viewmodel hands
        EnableViewmodelHands = config.GetSettingsValue<bool>("EnableViewmodelHands");

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

		Locator.GetPlayerCamera()?.nearClipPlane = FixItemClipping ? 0.05f : 0.1f;
    }

    private void Awake()
    {
        // create harmony patches
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        // set ModMain.Instance to be used by other classes (there should only ever be one ModMain instance at a time)
        Instance = this;
    }

    private void Start()
    {
        // check for other mods
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        HikersModAPI = ModHelper.Interaction.TryGetModApi<IHikersMod>("Owen013.MovementMod");

        // add components on scene load
        LoadManager.OnCompleteSceneLoad += (_, _) =>
        {
            ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                var player = Locator.GetPlayerBody();
                if (player == null) return;
                player.GetComponentInChildren<PlayerAnimController>().gameObject.AddComponent<AnimSpeedController>();

                var camera = Locator.GetPlayerCamera();
                camera.gameObject.AddComponent<OffsetManager>();
                camera.nearClipPlane = FixItemClipping ? 0.05f : 0.1f;

                ViewmodelArm.OnSceneLoad();
            });
        };

        // ready
        ModHelper.Console.WriteLine($"Immersion is ready to go!", MessageType.Success);
    }
}
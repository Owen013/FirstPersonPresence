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

    // config

    public bool IsHikersModInstalled { get; private set; }

    public bool EnableViewmodelHands { get; private set; }

    public bool EnableHeadBob { get; private set; }

    public float HeadBobStrength { get; private set; }

    public bool EnableToolBob { get; private set; }

    public float ToolBobStrength { get; private set; }

    public bool EnableDynamicToolPos { get; private set; }

    public float DynamicToolPosStrength { get; private set; }

    public bool EnableToolSway { get; private set; }

    public float ToolSwayStrength { get; private set; }

    public bool TweakItemPos { get; private set; }

    public bool EnableScoutAnim { get; private set; }

    public bool EnableLandingAnim { get; private set; }

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
        EnableToolBob = config.GetSettingsValue<bool>("EnableToolBob");
        ToolBobStrength = config.GetSettingsValue<float>("ToolBobStrength");

        // dynamic tool pos
        EnableDynamicToolPos = config.GetSettingsValue<bool>("EnableDynamicToolPos");
        DynamicToolPosStrength = config.GetSettingsValue<float>("DynamicToolPosStrength");

        // tool sway
        EnableToolSway = config.GetSettingsValue<bool>("EnableToolSway");
        ToolSwayStrength = config.GetSettingsValue<float>("ToolSwayStrength");

        // misc
        TweakItemPos = config.GetSettingsValue<bool>("TweakItemPos");
        EnableScoutAnim = config.GetSettingsValue<bool>("EnableScoutAnim");
        EnableLandingAnim = config.GetSettingsValue<bool>("EnableLandingAnim");
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
        IsHikersModInstalled = ModHelper.Interaction.ModExists("Owen013.MovementMod");

        // add components on scene load
        LoadManager.OnCompleteSceneLoad += (_, _) =>
        {
            ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                var player = Locator.GetPlayerBody();
                if (player == null) return;
                player.GetComponentInChildren<PlayerAnimController>().gameObject.AddComponent<AnimSpeedController>();
                Locator.GetPlayerCameraController().gameObject.AddComponent<ViewbobController>();

                ViewmodelArm.OnSceneLoad();
            });
        };

        // ready
        ModHelper.Console.WriteLine($"Immersion is ready to go!", MessageType.Success);
    }
}
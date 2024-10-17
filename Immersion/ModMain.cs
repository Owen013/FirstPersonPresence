using HarmonyLib;
using Immersion.Interfaces;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public static ISmolHatchling SmolHatchlingAPI { get; private set; }

    public static bool IsHikersModInstalled { get; private set; }

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

    public static string DynamicToolPosBehavior { get; private set; }

    public static float DynamicToolPosYAmount { get; private set; }

    public static float DynamicToolPosZAmount { get; private set; }

    public static bool IsJumpAnimEnabled { get; private set; }

    public static bool IsFallAnimEnabled { get; private set; }

    public static bool IsLandingAnimEnabled { get; private set; }

    public static bool IsScoutAnimEnabled { get; private set; }

    public static bool IsHideStowedItemsEnabled { get; private set; }

    public static bool IsLeftyModeEnabled { get; private set; }

    public delegate void ConfigureEvent();

    public event ConfigureEvent OnConfigure;

    public static void Print(string text, MessageType messageType = MessageType.Message)
    {
        if (Instance == null || Instance.ModHelper == null) return;
        Instance.ModHelper.Console.WriteLine(text, messageType);
    }

    public override object GetApi()
    {
        return new ImmersionAPI();
    }

    public override void Configure(IModConfig config)
    {
        base.Configure(config);

        IsViewModelHandsEnabled = config.GetSettingsValue<bool>("EnableViewmodelHands");
        IsViewBobEnabled = config.GetSettingsValue<bool>("EnableViewBob");
        ViewBobXAmount = config.GetSettingsValue<float>("ViewBobXAmount");
        ViewBobYAmount = config.GetSettingsValue<float>("ViewBobYAmount");
        ViewBobRollAmount = config.GetSettingsValue<float>("ViewBobRollAmount");
        ViewBobPitchAmount = config.GetSettingsValue<float>("ViewBobPitchAmount");
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
        IsJumpAnimEnabled = config.GetSettingsValue<bool>("UseJumpAnim");
        IsFallAnimEnabled = config.GetSettingsValue<bool>("UseFallAnim");
        IsLandingAnimEnabled = config.GetSettingsValue<bool>("UseLandingAnim");
        IsScoutAnimEnabled = config.GetSettingsValue<bool>("UseScoutAnim");
        IsHideStowedItemsEnabled = config.GetSettingsValue<bool>("HideStowedItems");
        IsLeftyModeEnabled = config.GetSettingsValue<bool>("UseLeftyMode");

        OnConfigure?.Invoke();
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        IsHikersModInstalled = ModHelper.Interaction.ModExists("Owen013.MovementMod");
        Print($"Immersion is ready to go!", MessageType.Success);
    }
}
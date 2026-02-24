using HarmonyLib;
using Immersion.APIs;
using Immersion.Components;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion;

public class ModMain : ModBehaviour
{
    public static ModMain Instance { get; private set; }

    public static ISmolHatchling SmolHatchlingAPI { get; private set; }

    public static IHikersMod HikersModAPI { get; private set; }

    public static bool IsTheStrangerTheyAreInstalled;

    public static void Log(string message, MessageType type = MessageType.Message)
    {
        Instance.ModHelper.Console.WriteLine(message, type);
    }

    public override object GetApi()
    {
        // provide API for use by other mods
        return new ImmersionAPI();
    }

    public override void Configure(IModConfig config)
    {
        Config.Configure(config);
		Locator.GetPlayerCamera()?.nearClipPlane = Config.FixItemClipping ? 0.05f : 0.1f;
    }

    private void Awake()
    {
        Instance = this;
        new Harmony("Owen_013.FirstPersonPresence").PatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        // check for other mods
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        HikersModAPI = ModHelper.Interaction.TryGetModApi<IHikersMod>("Owen013.MovementMod");
        IsTheStrangerTheyAreInstalled = ModHelper.Interaction.ModExists("AnonymousStrangerOW.TheStrangerTheyAre");

        // load assets
        ViewmodelArm.LoadAssetBundle();
        ArmData.LoadArmData();

        // ready
        ModHelper.Console.WriteLine($"Immersion is ready to go!", MessageType.Success);
    }
}
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

    public static ISmolHatchling SmolHatchlingAPI { get; private set; }

    public static IHikersMod HikersModAPI { get; private set; }

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

    public static void Log(string message, MessageType type = MessageType.Message)
    {
        Instance.ModHelper.Console.WriteLine(message, type);
    }

    private void Awake()
    {
        // create harmony patches
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        // set ModMain.Instance to be used by other classes (there should only ever be one ModMain instance at a time)
        Instance = this;

        LoadManager.OnCompleteSceneLoad += (_, _) =>
        {
            ViewmodelArm.LoadAssetBundleIfNull();
        };
    }

    private void Start()
    {
        // check for other mods
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        HikersModAPI = ModHelper.Interaction.TryGetModApi<IHikersMod>("Owen013.MovementMod");

        // ready
        ModHelper.Console.WriteLine($"Immersion is ready to go!", MessageType.Success);
    }
}
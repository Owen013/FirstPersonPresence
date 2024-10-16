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
        Config.UpdateConfig(config);
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
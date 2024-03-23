using HarmonyLib;
using Immersion.APIs;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion;

public class Main : ModBehaviour
{
    public static Main Instance;
    public ISmolHatchling SmolHatchlingAPI;

    public override object GetApi()
    {
        return new ImmersionAPI();
    }

    public override void Configure(IModConfig config)
    {
        Config.UpdateConfig(config);
    }

    public void WriteLine(string text, MessageType type = MessageType.Message)
    {
        ModHelper.Console.WriteLine(text, type);
    }

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        WriteLine($"Immersion is ready to go!", MessageType.Success);
    }
}

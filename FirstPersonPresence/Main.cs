using HarmonyLib;
using HikersMod.APIs;
using OWML.Common;
using OWML.ModHelper;

namespace FirstPersonPresence;

public class Main : ModBehaviour
{
    public static Main Instance;
    public ISmolHatchling SmolHatchlingAPI;

    // Config
    public float viewBobXSensitivity;
    public float viewBobYSensitivity;
    public float toolBobSensitivity;
    public float toolHeightYSensitivity;
    public float toolHeightZSensitivity;
    public bool debugLogEnabled;

    private void Awake()
    {
        Instance = this;
        Harmony.CreateAndPatchAll(typeof(Main));
    }

    private void Start()
    {
        SmolHatchlingAPI = ModHelper.Interaction.TryGetModApi<ISmolHatchling>("Owen013.TeenyHatchling");
        DebugLog($"First Person Presence is ready to go!", MessageType.Success, true);
    }

    public override void Configure(IModConfig config)
    {
        viewBobXSensitivity = config.GetSettingsValue<float>("View Bob X Sensitivity");
        viewBobYSensitivity = config.GetSettingsValue<float>("View Bob Y Sensitivity");
        toolBobSensitivity = config.GetSettingsValue<float>("Tool Bob Sensitivity");
        toolHeightYSensitivity = config.GetSettingsValue<float>("Dynamic Tool Height Y Sensitivity");
        toolHeightZSensitivity = config.GetSettingsValue<float>("Dynamic Tool Height Z Sensitivity");
        debugLogEnabled = config.GetSettingsValue<bool>("Enable Debug Log");
    }

    public void DebugLog(string text, MessageType type = MessageType.Message, bool forceMessage = false)
    {
        if (!debugLogEnabled && !forceMessage) return;
        ModHelper.Console.WriteLine(text, type);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.Start))]
    private static void OnCharacterControllerStart(PlayerCharacterController __instance)
    {
        __instance.gameObject.AddComponent<Components.ViewBobController>();
    }
}
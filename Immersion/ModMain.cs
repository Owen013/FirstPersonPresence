using HarmonyLib;
using Immersion.Scripts;
using Immersion.Scripts.APIs;
using Immersion.Scripts.Components;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;

namespace Immersion
{
    public class ModMain : ModBehaviour
    {
        public static ModMain Instance { get; private set; }

        public static IModAssets Assets => Instance.ModHelper.Assets;

        public static IModEvents Events => Instance.ModHelper.Events;

        public static IModConsole Console => Instance.ModHelper.Console;

        public static ISmolHatchling SmolHatchlingAPI { get; private set; }

        public static IHikersMod HikersModAPI { get; private set; }

        public override void Configure(IModConfig config)
        {
            Config.Configure(config);
            Locator.GetPlayerCamera()?.nearClipPlane = Config.FixViewmodelClipping ? 0.05f : 0.1f;
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

            // load viewmodel arm stuff
            ArmData.Load();
            ViewmodelArm.LoadAsset();

            // ready
            Console.WriteLine($"Immersion is ready to go!", MessageType.Success);
        }
    }
}
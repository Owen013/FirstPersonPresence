using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class OffsetManager : MonoBehaviour
    {
        private static readonly Dictionary<string, float> s_itemOffsetScales = new Dictionary<string, float>
        {
            ["DreamLantern"] = 1.2f,
            ["DreamLantern_Malfunctioning"] = 1.2f,
            ["CloakMineral"] = 0.8f,
            ["StrangerSeal"] = 0.8f,
            ["GhostbirdSkull"] = 0.8f,
            ["Compass"] = 0.8f
        };

        private float _currentItemOffsetScale = 1f;

        public static OffsetManager Instance { get; private set; }

        public OffsetRoot CameraOffsetRoot { get; private set; }

        public OffsetRoot ItemToolOffsetRoot { get; private set; }

        public OffsetRoot SignalscopeOffsetRoot { get; private set; }

        public OffsetRoot ProbeLauncherOffsetRoot { get; private set; }

        public OffsetRoot TranslatorOffsetRoot { get; private set; }

        /// <summary>
        /// Applies a translational offset to the player camera.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        public void AddCameraOffset(Vector3 position)
        {
            CameraOffsetRoot.AddOffset(position);
        }

        /// <summary>
        /// Applies a rotational offset to the player camera.
        /// </summary>
        /// <param name="rotation">The local rotation of the offset.</param>
        public void AddCameraOffset(Quaternion rotation)
        {
            CameraOffsetRoot.AddOffset(rotation);
        }

        /// <summary>
        /// Applies a translational and rotational offset to the player camera.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        /// <param name="rotation">The local rotation of the offset.</param>
        public void AddCameraOffset(Vector3 position, Quaternion rotation)
        {
            AddCameraOffset(position);
            AddCameraOffset(rotation);
        }

        /// <summary>
        /// Applies a translational offset to all tools that is scaled to match the tool scale.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        public void AddToolOffsets(Vector3 position)
        {
            // apply different scaling factors for different tools
            ItemToolOffsetRoot.AddOffset(0.8f * _currentItemOffsetScale * position);
            SignalscopeOffsetRoot.AddOffset(position);
            ProbeLauncherOffsetRoot.AddOffset(3f * position);
            TranslatorOffsetRoot.AddOffset(3f * position);
        }

        /// <summary>
        /// Applies a rotational offset to all tools.
        /// </summary>
        /// <param name="rotation">The local rotation of the offset.</param>
        public void AddToolOffsets(Quaternion rotation)
        {
            // apply same rotation offset for all tools
            ItemToolOffsetRoot.AddOffset(rotation);
            SignalscopeOffsetRoot.AddOffset(rotation);
            ProbeLauncherOffsetRoot.AddOffset(rotation);
            TranslatorOffsetRoot.AddOffset(rotation);
        }

        /// <summary>
        /// Applies a translational offset (rescaling it so that it looks the same on all tools) and a rotational offset to all tools.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        /// <param name="rotation">The local rotation of the offset.</param>
        public void AddToolOffsets(Vector3 position, Quaternion rotation)
        {
            AddToolOffsets(position);
            AddToolOffsets(rotation);
        }

        internal static void OnPickUpItem(OWItem item)
        {
            string itemId = item.GetItemType().GetName();

            // some items require special treatment
            if (itemId == "DreamLantern" && item is DreamLanternItem dreamLantern && dreamLantern.GetLanternType() != DreamLanternType.Functioning)
            {
                itemId += $"_{dreamLantern.GetLanternType().GetName()}";
            }

            if (s_itemOffsetScales.ContainsKey(itemId))
            {
                Instance._currentItemOffsetScale = s_itemOffsetScales[itemId];
            }
            else
            {
                Instance._currentItemOffsetScale = 1f;
            }
        }

        private void Start()
        {
            Instance = this;

            // create offset roots
            CameraOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Camera", Locator.GetPlayerCamera().gameObject);
            var toolModeSwapper = Locator.GetToolModeSwapper();
            ItemToolOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ItemCarryTool", toolModeSwapper.GetItemCarryTool().gameObject);
            SignalscopeOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Signalscope", toolModeSwapper.GetSignalScope().gameObject);
            ProbeLauncherOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ProbeLauncher", toolModeSwapper.GetProbeLauncher().gameObject);
            TranslatorOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_NomaiTranslatorProp", toolModeSwapper.GetTranslator().gameObject);

            gameObject.AddComponent<ViewbobController>();
            gameObject.AddComponent<ViewmodelOffsetController>();
            gameObject.AddComponent<ViewmodelSwayController>();
            gameObject.AddComponent<BreathingAnimController>();
            gameObject.AddComponent<ScoutAnimController>();
            gameObject.AddComponent<LandingAnimController>();
            gameObject.AddComponent<HideStowedItemsController>();

            if (ModMain.HikersModAPI != null)
            {
                gameObject.AddComponent<SprintingAnimController>();
            }
        }
    }
}
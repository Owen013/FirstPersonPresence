using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class OffsetManager : MonoBehaviour
    {
        public static OffsetManager Instance { get; private set; }

        private static readonly Dictionary<string, float> s_itemOffsetScales = new Dictionary<string, float>
        {
            ["DreamLantern"] = 1.2f,
            ["DreamLantern_Malfunctioning"] = 1.2f,
            ["CloakMineral"] = 0.8f,
            ["StrangerSeal"] = 0.8f,
            ["GhostbirdSkull"] = 0.8f,
            ["Compass"] = 0.8f
        };

        private OffsetRoot _cameraOffsetRoot;

        private OffsetRoot _itemToolOffsetRoot;

        private OffsetRoot _signalscopeOffsetRoot;

        private OffsetRoot _probeLauncherOffsetRoot;

        private OffsetRoot _translatorOffsetRoot;

        private float _currentItemOffsetScale = 1f;

        /// <summary>
        /// Applies a translational offset to the player camera.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        public void AddCameraOffset(Vector3 position)
        {
            _cameraOffsetRoot.AddOffset(position);
        }

        /// <summary>
        /// Applies a rotational offset to the player camera.
        /// </summary>
        /// <param name="rotation">The local rotation of the offset.</param>
        public void AddCameraOffset(Quaternion rotation)
        {
            _cameraOffsetRoot.AddOffset(rotation);
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
        /// <param name="toolsToOffset">The tools to apply the offset to.</param>
        public void AddToolOffset(Vector3 position, Tools toolsToOffset)
        {
            // apply different scaling factors for different tools
            if (toolsToOffset.HasFlag(Tools.ItemTool))
            {
                _itemToolOffsetRoot.AddOffset(0.8f * _currentItemOffsetScale * position);
            }

            if (toolsToOffset.HasFlag(Tools.Signalscope))
            {
                _signalscopeOffsetRoot.AddOffset(position);
            }

            if (toolsToOffset.HasFlag(Tools.ProbeLauncher))
            {
                _probeLauncherOffsetRoot.AddOffset(3f * position);
            }

            if (toolsToOffset.HasFlag(Tools.Translator))
            {
                _translatorOffsetRoot.AddOffset(3f * position);
            }
        }

        /// <summary>
        /// Applies a rotational offset to all tools.
        /// </summary>
        /// <param name="rotation">The local rotation of the offset.</param>
        /// <param name="toolsToOffset">The tools to apply the offset to.</param>
        public void AddToolOffset(Quaternion rotation, Tools toolsToOffset)
        {
            // apply different scaling factors for different tools
            if (toolsToOffset.HasFlag(Tools.ItemTool))
            {
                _itemToolOffsetRoot.AddOffset(rotation);
            }

            if (toolsToOffset.HasFlag(Tools.Signalscope))
            {
                _signalscopeOffsetRoot.AddOffset(rotation);
            }

            if (toolsToOffset.HasFlag(Tools.ProbeLauncher))
            {
                _probeLauncherOffsetRoot.AddOffset(rotation);
            }

            if (toolsToOffset.HasFlag(Tools.Translator))
            {
                _translatorOffsetRoot.AddOffset(rotation);
            }
        }

        /// <summary>
        /// Applies a translational offset (rescaling it so that it looks the same on all tools) and a rotational offset to all tools.
        /// </summary>
        /// <param name="position">The local position of the offset.</param>
        /// <param name="rotation">The local rotation of the offset.</param>
        /// <param name="toolsToOffset">The tools to apply the offset to.</param>
        public void AddToolOffset(Vector3 position, Quaternion rotation, Tools toolsToOffset)
        {
            AddToolOffset(position, toolsToOffset);
            AddToolOffset(rotation, toolsToOffset);
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
            _cameraOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Camera", Locator.GetPlayerCamera().gameObject);
            var toolModeSwapper = Locator.GetToolModeSwapper();
            _itemToolOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ItemCarryTool", toolModeSwapper.GetItemCarryTool().gameObject);
            _signalscopeOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Signalscope", toolModeSwapper.GetSignalScope().gameObject);
            _probeLauncherOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ProbeLauncher", toolModeSwapper.GetProbeLauncher().gameObject);
            _translatorOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_NomaiTranslatorProp", toolModeSwapper.GetTranslator().gameObject);

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
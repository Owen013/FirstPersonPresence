using OWML.Utils;
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class HideStowedItemsController : ToggleableBehaviour
    {
        private OffsetManager _offsetManager;

        private ToolModeSwapper _toolModeSwapper;

        private float _addedStowDegrees;

        protected override void OnConfigured()
        {
            enabled = Config.HideStowedItems;
        }

        protected override void Awake()
        {
            base.Awake();
            _offsetManager = OffsetManager.Instance;
            _toolModeSwapper = Locator.GetToolModeSwapper();
        }

        private void Update()
        {
            var itemCarryTool = _toolModeSwapper.GetItemCarryTool();
            var heldItem = itemCarryTool.GetHeldItem();
            if (heldItem != null)
            {
                // compass item is not supposed to be stowed when at the cockpit
                bool holdingCompassInShip = heldItem.GetItemType().GetName() == "Compass" && OWInput.IsInputMode(InputMode.ShipCockpit);
                if (!holdingCompassInShip && !itemCarryTool.IsPuttingAway() && _toolModeSwapper.GetToolMode() != ToolMode.Item)
                {
                    // tilt item carry tool further offscreen once its vanilla stow animation finishes
                    float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
                    _addedStowDegrees = Mathf.MoveTowards(_addedStowDegrees, 45f, 135f * deltaTime);
                    var offsetRotation = Quaternion.AngleAxis(_addedStowDegrees, Vector3.right);
                    _offsetManager.AddToolOffset(offsetRotation, Tools.ItemTool);
                    return;
                }
            }

            _addedStowDegrees = 0f;
        }

        private void OnDisable()
        {
            _addedStowDegrees = 0f;
        }
    }
}
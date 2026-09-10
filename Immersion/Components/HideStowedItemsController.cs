using OWML.Utils;
using UnityEngine;

namespace Immersion.Components;

public class HideStowedItemsController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private ToolModeSwapper _toolModeSwapper;

    private float _addedStowDegrees;

    private void OnConfigured()
    {
        enabled = Config.HideStowedItems;
    }

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _toolModeSwapper = Locator.GetToolModeSwapper();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    private void Update()
    {
        var itemCarryTool = _toolModeSwapper.GetItemCarryTool();
        OWItem heldItem = itemCarryTool.GetHeldItem();

        // Compass item is not supposed to be stowed when at the cockpit.
        bool isHoldingCompass = heldItem != null && heldItem.GetItemType().GetName() == "Compass";
        bool isUsingCompassAtCockpit = isHoldingCompass && OWInput.IsInputMode(InputMode.ShipCockpit);

        bool isItemStowed = _toolModeSwapper.GetToolMode() != ToolMode.Item && !itemCarryTool.IsPuttingAway();
        if (!isUsingCompassAtCockpit && isItemStowed)
        {
            // Tilt item carry tool further offscreen once its vanilla stow animation finishes.
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            _addedStowDegrees = Mathf.MoveTowards(_addedStowDegrees, 45f, 135f * deltaTime);
            var offsetRotation = Quaternion.AngleAxis(_addedStowDegrees, Vector3.right);
            _offsetManager.AddToolOffset(offsetRotation, Tools.ItemTool);
        }
        else
        {
            _addedStowDegrees = 0f;
        }
    }

    private void OnDisable()
    {
        _addedStowDegrees = 0f;
    }

    private void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
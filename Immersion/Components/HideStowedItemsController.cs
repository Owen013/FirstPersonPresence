using UnityEngine;

namespace Immersion.Components;

public class HideStowedItemsController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private ToolModeSwapper _toolModeSwapper;

    private float _addedStowDegrees;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _toolModeSwapper = Locator.GetToolModeSwapper();
    }

    private void Update()
    {
        var itemCarryTool = _toolModeSwapper.GetItemCarryTool();
        if (Config.HideStowedItems && itemCarryTool._heldItem != null && !itemCarryTool.IsPuttingAway() && _toolModeSwapper.GetToolMode() != ToolMode.Item)
        {
            _addedStowDegrees = Mathf.MoveTowards(_addedStowDegrees, 45f, 135f * Time.deltaTime);
            _offsetManager.ItemToolOffsetRoot.AddOffset(Quaternion.Euler(_addedStowDegrees, 0f, 0f));
        }
        else
            _addedStowDegrees = 0f;
    }
}

using UnityEngine;

namespace Immersion.Scripts.Components;

public class HideStowedItemsController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private ToolModeSwapper _toolModeSwapper;

    private float _addedStowDegrees;

    private void OnConfigured() =>
        enabled = Config.HideStowedItems;

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
        if (itemCarryTool.GetHeldItem() != null && !itemCarryTool.IsPuttingAway() && _toolModeSwapper.GetToolMode() != ToolMode.Item)
        {
            // tilt item carry tool further offscreen once its vanilla stow animation finishes
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            _addedStowDegrees = Mathf.MoveTowards(_addedStowDegrees, 45f, 135f * deltaTime);
            _offsetManager.ItemToolOffsetRoot.AddOffset(Quaternion.Euler(_addedStowDegrees, 0f, 0f));
        }
        else
            _addedStowDegrees = 0f;
    }

    private void OnDisable() =>
        _addedStowDegrees = 0f;

    private void OnDestroy() =>
        Config.OnConfigured -= OnConfigured;
}
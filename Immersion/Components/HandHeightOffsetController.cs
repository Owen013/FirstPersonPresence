using UnityEngine;

namespace Immersion.Components;

public class HandHeightOffsetController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCameraController _cameraController;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _cameraController = Locator.GetPlayerCameraController();
    }

    private void Update()
    {
        // only do this if dynamic tool position is enabled and strength is non-zero
        if (Config.EnableHandHeightOffset)
        {
            float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
            Vector3 toolOffset = Vector3.zero;
            // trig is used for circular motion
            // tool moves down+back when looking up, and up+back when looking down
            // tool is not offset when looking straight ahead
            toolOffset.z = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1;
            toolOffset.y = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            _offsetManager.AddToolOffsets(Config.HandHeightOffsetStrength * 0.05f * toolOffset);
        }
    }
}

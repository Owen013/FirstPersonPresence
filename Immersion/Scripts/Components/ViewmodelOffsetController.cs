using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewmodelOffsetController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private PlayerCameraController _cameraController;

        private void OnConfigured()
        {
            enabled = Config.EnableViewmodelOffset;
        }

        private void Awake()
        {
            _offsetManager = OffsetManager.Instance;
            _cameraController = Locator.GetPlayerCameraController();

            Config.OnConfigured += OnConfigured;
            OnConfigured();
        }

        private void Update()
        {
            float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
            Vector3 toolOffset = Vector3.zero;
            // trig is used for circular motion
            // tool moves down+back when looking up, and up+back when looking down
            // tool is not offset when looking straight ahead
            toolOffset.z = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1f;
            toolOffset.y = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            _offsetManager.AddToolOffsets(Config.ViewmodelOffsetStrength * 0.05f * toolOffset);
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
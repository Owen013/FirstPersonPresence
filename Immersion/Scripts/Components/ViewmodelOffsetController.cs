using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewmodelOffsetController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private PlayerCameraController _cameraController;

        private float MaxDisplacement => 0.05f * Config.ViewmodelOffsetScale;

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
            var offset = Vector3.zero;
            offset.y = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            offset.z = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1f;
            _offsetManager.AddToolOffsets(MaxDisplacement * offset);
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewmodelOffsetController : ToggleableBehaviour
    {
        private OffsetManager _offsetManager;

        private PlayerCameraController _cameraController;

        private float MaxDisplacement => 0.05f * Config.ViewmodelOffsetScale;

        protected override void OnConfigured()
        {
            enabled = Config.EnableViewmodelOffset;
        }

        protected override void Awake()
        {
            base.Awake();
            _offsetManager = OffsetManager.Instance;
            _cameraController = Locator.GetPlayerCameraController();
        }

        private void Update()
        {
            float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
            float offsetY = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            float offsetZ = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1f;
            _offsetManager.AddToolOffset(MaxDisplacement * new Vector3(0f, offsetY, offsetZ), Tools.All);
        }
    }
}
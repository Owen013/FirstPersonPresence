using UnityEngine;

namespace Immersion.Components;

public class ViewmodelOffsetController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCameraController _cameraController;

    private float MaxOffsetScale => 0.05f * Config.ViewmodelOffsetScale;

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
        float offsetY = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
        float offsetZ = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1f;
        _offsetManager.AddToolOffset(MaxOffsetScale * new Vector3(0f, offsetY, offsetZ), Tools.All);
    }

    private void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
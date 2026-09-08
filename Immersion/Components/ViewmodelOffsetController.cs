using UnityEngine;

namespace Immersion.Components;

class ViewmodelOffsetController : MonoBehaviour
{
    OffsetManager _offsetManager;

    PlayerCameraController _cameraController;

    float MaxOffsetScale => 0.05f * Config.ViewmodelOffsetScale;

    void OnConfigured()
    {
        enabled = Config.EnableViewmodelOffset;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _cameraController = Locator.GetPlayerCameraController();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
        float offsetY = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
        float offsetZ = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1f;
        _offsetManager.AddToolOffset(MaxOffsetScale * new Vector3(0f, offsetY, offsetZ), Tools.All);
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
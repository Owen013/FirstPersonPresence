using UnityEngine;

namespace Immersion.Components;

class ViewmodelSwayController : MonoBehaviour
{
    OffsetManager _offsetManager;

    PlayerCameraController _cameraController;

    PlayerCharacterController _playerController;

    Vector2 _currentSway;

    Vector2 _swayVelocity;

    float MaxDisplacement => 0.25f * Config.ViewmodelSwayScale;

    void OnConfigured()
    {
        enabled = Config.EnableViewmodelSway;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _cameraController = Locator.GetPlayerCameraController();
        _playerController = Locator.GetPlayerController();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        float degreesY = _cameraController.GetDegreesY();
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f)
        {
            _currentSway = Vector2.SmoothDamp(_currentSway, Vector2.zero, ref _swayVelocity, 0.2f, Mathf.Infinity, deltaTime);

            if (OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
            {
                Vector2 lookInput = OWInput.GetAxisValue(InputLibrary.look);
                lookInput *= _cameraController._playerCamera.fieldOfView / _cameraController._initFOV;
                lookInput *= InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : deltaTime;
                var alarmController = Locator.GetAlarmSequenceController();
                bool isAlarmWakingPlayer = alarmController != null && alarmController.IsAlarmWakingPlayer();
                if (_cameraController._zoomed || isAlarmWakingPlayer)
                    lookInput *= PlayerCameraController.ZOOM_SCALAR;

                if (_playerController._isTurningLocked)
                    lookInput.x = 0f;
                else
                    lookInput.x *= (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;

                if (degreesY >= PlayerCameraController._maxDegreesYNormal && lookInput.y > 0f)
                    lookInput.y = 0f;
                else if (degreesY <= PlayerCameraController._minDegreesYNormal && lookInput.y < 0f)
                    lookInput.y = 0f;

                _currentSway += -lookInput * (0.5f * Mathf.Cos(Mathf.PI * _currentSway.magnitude) + 0.5f);
            }
        }

        float xScale = Mathf.Sqrt(Mathf.Clamp01(1f - _currentSway.x * _currentSway.x));
        float yScale = Mathf.Sqrt(Mathf.Clamp01(1f - _currentSway.y * _currentSway.y));
        var swayX = Vector3.right * _currentSway.x * xScale;
        var swayY = Vector3.up * _currentSway.y * yScale;
        var swayCameraZ = Vector3.forward * (yScale - 1f);
        var swayPlayerZ = _cameraController.transform.InverseTransformDirection(_playerController.transform.forward) * (xScale - 1f);

        var offset = MaxDisplacement * (swayX + swayY + swayCameraZ + swayPlayerZ);
        _offsetManager.AddToolOffset(offset, Tools.All);
    }

    void OnDisable()
    {
        _currentSway = Vector3.zero;
        _swayVelocity = Vector3.zero;
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
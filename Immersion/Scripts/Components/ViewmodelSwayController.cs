using UnityEngine;

namespace Immersion.Scripts.Components;

public class ViewmodelSwayController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCameraController _cameraController;

    private PlayerCharacterController _playerController;

    private Vector2 _currentSway;

    private Vector2 _swayVelocity;

    private void OnConfigured() =>
        enabled = Config.EnableViewmodelSway;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _cameraController = Locator.GetPlayerCameraController();
        _playerController = Locator.GetPlayerController();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    private void Update()
    {
        float degreesY = _cameraController.GetDegreesY();

        // only add new sway if player is in ground movement mode and the game is unpaused
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f && OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
        {
            // get look input
            Vector2 lookInput = OWInput.GetAxisValue(InputLibrary.look);
            lookInput *= _cameraController._playerCamera.fieldOfView / _cameraController._initFOV;
            lookInput *= InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : deltaTime;
            bool isAlarmWakingPlayer = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            if (_cameraController._zoomed || isAlarmWakingPlayer)
                lookInput *= PlayerCameraController.ZOOM_SCALAR;

            // player can't turn left or right if turning is locked
            if (_playerController._isTurningLocked)
                lookInput.x = 0f;

            // horizontal sway is reduced the more up/down player is looking
            lookInput.x *= (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;

            // cancel out vertical sway if player is at max or min vertical look angle and is trying to turn more in that direction
            if (degreesY >= PlayerCameraController._maxDegreesYNormal)
                lookInput.y = Mathf.Min(0f, lookInput.y);
            if (degreesY <= PlayerCameraController._minDegreesYNormal)
                lookInput.y = Mathf.Max(0f, lookInput.y);

            // decay tool sway
            _currentSway = Vector2.SmoothDamp(_currentSway, Vector2.zero, ref _swayVelocity, 0.2f, Mathf.Infinity, deltaTime);

            // add new tool sway
            _currentSway += -lookInput * (0.5f * Mathf.Cos(Mathf.PI * _currentSway.magnitude) + 0.5f);
        }

        // x sway is less pronounced the more up/down the player is looking
        // sway is split into local (relative to camera) and global (relative to player)
        var inwardOffset = Vector3.forward * (Mathf.Sqrt(Mathf.Clamp01(1f - _currentSway.y * _currentSway.y)) - 1f);
        var relativePlayerForward = _cameraController.transform.InverseTransformDirection(_playerController.transform.forward);
        var backwardOffset = relativePlayerForward * (Mathf.Sqrt(Mathf.Clamp01(1f - _currentSway.x * _currentSway.x)) - 1f);

        // calculate and apply the final offset
        var offset = new Vector3(_currentSway.x, _currentSway.y, 0f) + inwardOffset + backwardOffset;
        offset *= 0.25f * Config.ViewmodelSwayStrength;
        _offsetManager.AddToolOffsets(offset);
    }

    private void OnDisable()
    {
        // if tool sway is disabled, reset tool sway parameters
        _currentSway = Vector3.zero;
        _swayVelocity = Vector3.zero;
    }

    private void OnDestroy() =>
        Config.OnConfigured -= OnConfigured;
}
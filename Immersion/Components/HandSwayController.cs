using UnityEngine;

namespace Immersion.Components;

public class HandSwayController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCameraController _cameraController;

    private PlayerCharacterController _playerController;

    private Vector2 _handSway;

    private Vector2 _handSwayDampVel;

    private void OnConfigured()
    {
        enabled = Config.EnableHandSway;
    }

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
        if (Time.deltaTime != 0f && OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
        {
            // get look input
            Vector2 lookInput = OWInput.GetAxisValue(InputLibrary.look);
            lookInput *= _cameraController._playerCamera.fieldOfView / _cameraController._initFOV;
            lookInput *= InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : Time.deltaTime;
            bool isAlarmWakingPlayer = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            if (_cameraController._zoomed || isAlarmWakingPlayer)
                lookInput *= PlayerCameraController.ZOOM_SCALAR;

            // player can't turn left or right if turning is locked
            if (_playerController._isTurningLocked)
            {
                lookInput.x = 0f;
            }

            // horizontal sway is reduced the more up/down player is looking
            lookInput.x *= (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;

            // cancel out vertical sway if player is at max or min vertical look angle and is trying to turn more in that direction
            if (degreesY >= PlayerCameraController._maxDegreesYNormal)
                lookInput.y = Mathf.Min(0f, lookInput.y);
            if (degreesY <= PlayerCameraController._minDegreesYNormal)
                lookInput.y = Mathf.Max(0f, lookInput.y);

            // decay already existing tool sway and then add new tool sway
            _handSway -= lookInput * (1f - Mathf.Min((_handSway - lookInput).magnitude, 1));
        }

        // x sway is less pronounced the more up/down the player is looking
        // sway is split into local (relative to camera) and global (relative to player)
        float localZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _handSway.y) - 1f);
        float globalZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _handSway.x) - 1f);

        // calculate and apply the final offset
        var offset = new Vector3(_handSway.x, _handSway.y, localZOffset);
        offset += globalZOffset * _cameraController.transform.InverseTransformDirection(_playerController.transform.forward);
        offset *= Config.HandSwayStrength * 0.25f;
        _offsetManager.AddToolOffsets(offset);

        // decay tool sway
        _handSway = Vector2.SmoothDamp(_handSway, Vector2.zero, ref _handSwayDampVel, 0.2f);
    }

    private void OnDisable()
    {
        // if tool sway is disabled, reset tool sway parameters
        _handSway = Vector3.zero;
        _handSwayDampVel = Vector3.zero;
    }

    private void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
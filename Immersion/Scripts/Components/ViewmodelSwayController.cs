using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewmodelSwayController : ToggleableBehaviour
    {
        private OffsetManager _offsetManager;

        private PlayerCameraController _cameraController;

        private PlayerCharacterController _playerController;

        private Vector2 _currentSway;

        private Vector2 _velocity;

        private float MaxDisplacement => 0.25f * Config.ViewmodelSwayScale;

        protected override void OnConfigured()
        {
            enabled = Config.EnableViewmodelSway;
        }

        protected override void Awake()
        {
            base.Awake();
            _offsetManager = OffsetManager.Instance;
            _cameraController = Locator.GetPlayerCameraController();
            _playerController = Locator.GetPlayerController();
        }

        private void Update()
        {
            float degreesY = _cameraController.GetDegreesY();

            // only add new sway if player is in ground movement mode and the game is unpaused
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime != 0f)
            {
                // decay sway
                _currentSway = Vector2.SmoothDamp(_currentSway, Vector2.zero, ref _velocity, 0.2f, Mathf.Infinity, deltaTime);

                if (OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
                {
                    // get look input
                    Vector2 lookInput = OWInput.GetAxisValue(InputLibrary.look);
                    lookInput *= _cameraController._playerCamera.fieldOfView / _cameraController._initFOV;
                    lookInput *= InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : deltaTime;
                    var alarmController = Locator.GetAlarmSequenceController();
                    bool isAlarmWakingPlayer = alarmController != null && alarmController.IsAlarmWakingPlayer();
                    if (_cameraController._zoomed || isAlarmWakingPlayer)
                    {
                        lookInput *= PlayerCameraController.ZOOM_SCALAR;
                    }

                    // player can't turn left or right if turning is locked
                    if (_playerController._isTurningLocked)
                    {
                        lookInput.x = 0f;
                    }
                    else
                    {
                        // horizontal sway is reduced the more up/down player is looking
                        lookInput.x *= (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;
                    }

                    // player can't look up/down if at max/min degrees
                    if (degreesY >= PlayerCameraController._maxDegreesYNormal && lookInput.y > 0f)
                    {
                        lookInput.y = 0f;
                    }
                    else if (degreesY <= PlayerCameraController._minDegreesYNormal && lookInput.y < 0f)
                    {
                        lookInput.y = 0f;
                    }

                    // add new sway
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

        private void OnDisable()
        {
            _currentSway = Vector3.zero;
            _velocity = Vector3.zero;
        }
    }
}
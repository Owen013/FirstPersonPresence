using UnityEngine;

namespace Immersion.Components;

public class OffsetManager : MonoBehaviour
{
    private PlayerCameraController _cameraController;

    private PlayerCharacterController _playerController;

    private PlayerAnimController _animController;

    private ToolModeSwapper _toolModeSwapper;

    private OffsetRoot _cameraOffsetRoot;

    private OffsetRoot _itemToolOffsetRoot;

    private OffsetRoot _signalscopeOffsetRoot;

    private OffsetRoot _probeLauncherOffsetRoot;

    private OffsetRoot _translatorOffsetRoot;

    private float _viewbobTime;

    private float _viewbobScale;

    private float _viewbobDampVel;

    private Vector2 _handSway;

    private Vector2 _handSwayDampVel;

    private Vector3 _breathingAnimPos;

    private Vector3 _breathingAnimTargetPos;

    private Vector3 _breathingAnimDampVel;

    private float _breathingAnimNextUpdateTime;

    private float _lastScoutLaunchTime;

    private bool _isScoutAnimActive;

    private float _scoutAnimStrength;

    private float _scoutAnimVel;

    private Vector3 _lastPlayerVel;

    private bool _isLandingAnimActive;

    private float _lastLandedSpeed;

    private float _landingAnimPos;

    private float _landingAnimDampVel;

    private float _sprintAnimScale;

    private float _sprintAnimDampVel;

    internal static void AddToPlayerCamera(PlayerCameraController playerCamera)
    {
        playerCamera.gameObject.AddComponent<OffsetManager>();
        playerCamera._playerCamera.nearClipPlane = Config.FixItemClipping ? 0.05f : 0.1f;
    }

    private void Awake()
    {
        // get references to required components
        _cameraController = Locator.GetPlayerCameraController();
        _playerController = Locator.GetPlayerController();
        _animController = _playerController.GetComponentInChildren<PlayerAnimController>();
        _toolModeSwapper = Locator.GetToolModeSwapper();

        // create offset roots
        _cameraOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Camera", _cameraController.gameObject);
        var camera = _cameraController._playerCamera.transform;
        _itemToolOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ItemCarryTool", camera.Find("ItemCarryTool").gameObject);
        _signalscopeOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_Signalscope", camera.Find("Signalscope").gameObject);
        _probeLauncherOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_ProbeLauncher", camera.Find("ProbeLauncher").gameObject);
        _translatorOffsetRoot = OffsetRoot.NewOffsetRoot("OffsetRoot_NomaiTranslatorTranslatorProp", camera.Find("NomaiTranslatorProp").gameObject);

        _playerController.OnBecomeGrounded += () =>
        {
            // if the player lands with a downward speed of at least 5, play landing anim
            Vector3 landingVel = _lastPlayerVel - _playerController.GetGroundBody().GetPointVelocity(_playerController.GetGroundContactPoint());
            float landingSpeed = -_playerController.transform.InverseTransformVector(landingVel).y;

            if (ModMain.SmolHatchlingAPI != null)
            {
                // avoid dividing by 0
                float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                if (playerScale != 0f)
                    landingSpeed /= ModMain.SmolHatchlingAPI.GetPlayerScale();
            }

            if (landingSpeed >= 5f)
            {
                _lastLandedSpeed = landingSpeed;
                _isLandingAnimActive = true;
            }
        };

        _probeLauncherOffsetRoot.GetComponentInChildren<ProbeLauncher>().OnLaunchProbe += (_) =>
        {
            // play scout launcher animation if enabled
            if (Config.EnableScoutAnim)
            {
                _isScoutAnimActive = true;
                _lastScoutLaunchTime = Time.time;
            }
        };
    }

    private void AddToolOffsets(Vector3 position)
    {
        // apply different scaling factors for different tools
        _itemToolOffsetRoot.AddOffset(position);
        _signalscopeOffsetRoot.AddOffset(position);
        _probeLauncherOffsetRoot.AddOffset(3f * position);
        _translatorOffsetRoot.AddOffset(3f * position);
    }

    private void AddToolOffsets(Quaternion rotation)
    {
        // apply same rotation offset for all tools
        _itemToolOffsetRoot.AddOffset(rotation);
        _signalscopeOffsetRoot.AddOffset(rotation);
        _probeLauncherOffsetRoot.AddOffset(rotation);
        _translatorOffsetRoot.AddOffset(rotation);
    }

    private void AddToolOffsets(Vector3 position, Quaternion rotation)
    {
        AddToolOffsets(position);
        AddToolOffsets(rotation);
    }

    private void UpdateViewbob()
    {
        // only do this if player is not movement locked and viewbob is enabled for camera or tool
        if (!_playerController._isMovementLocked && (Config.EnableHeadBob || Config.EnableHandBob))
        {
            if (Time.deltaTime != 0f)
            {
                // viewbob cycle increases based on player ground speed
                // viewbob time and viewbob strength are used by both camera and tool bobbing
                _viewbobTime += _animController._animator.speed * Time.deltaTime;
                if (_playerController.IsGrounded())
                {
                    // change viewbob strength quickly if on ground
                    Vector3 groundVel = _playerController.GetRelativeGroundVelocity();
                    groundVel.y = 0f;

                    if (ModMain.SmolHatchlingAPI != null)
                    {
                        // avoid dividing by 0
                        float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                        if (playerScale != 0f)
                            groundVel /= ModMain.SmolHatchlingAPI.GetPlayerScale();
                    }

                    _viewbobScale = Mathf.SmoothDamp(_viewbobScale, Mathf.Min(groundVel.magnitude / 6f, 2f), ref _viewbobDampVel, 0.05f);
                }
                else
                {
                    // decay viewbob strength slowly if in air
                    _viewbobScale = Mathf.SmoothDamp(_viewbobScale, 0f, ref _viewbobDampVel, 1f);
                }
            }

            // trig is used for a circular viewbob motion
            var viewBob = _viewbobScale * new Vector2(Mathf.Sin(_viewbobTime * 2f * Mathf.PI), Mathf.Cos(_viewbobTime * 4f * Mathf.PI));

            // apply camera offset if camera bob is enabled
            if (Config.EnableHeadBob)
                _cameraOffsetRoot.AddOffset(Config.HeadBobStrength * 0.02f * new Vector3(viewBob.x, viewBob.y));

            // apply tool offset if tool bob is enabled
            if (Config.EnableHandBob)
            {
                var offsetPos = Config.HandBobStrength * new Vector3(0.02f * viewBob.x, 0.003f * viewBob.y);
                var offsetRot = Quaternion.Euler(Config.HandBobStrength * _viewbobScale * -0.75f * Mathf.Sin(_viewbobTime * 4f * Mathf.PI), 0f, 0f);
                AddToolOffsets(offsetPos, offsetRot);
            }
        }
        // reset viewbob parameters if both camera and tool bob are disabled
        else
        {
            _viewbobTime = 0f;
            _viewbobScale = 0f;
            _viewbobDampVel = 0f;
        }
    }

    private void UpdateHandHeightOffset()
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
            AddToolOffsets(Config.HandHeightOffsetStrength * 0.05f * toolOffset);
        }
    }

    private void UpdateHandSway()
    {
        // only do this if tool sway is enabled
        if (Config.EnableHandSway)
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
            float xSwayScale = (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;
            float localZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _handSway.y) - 1f);
            float globalZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _handSway.x) - 1f);

            // calculate and apply the final offset
            var offset = new Vector3(_handSway.x * xSwayScale, _handSway.y, localZOffset);
            offset += xSwayScale * globalZOffset * _cameraController.transform.InverseTransformDirection(_playerController.transform.forward);
            offset *= Config.HandSwayStrength * 0.25f;
            AddToolOffsets(offset);

            // decay tool sway
            _handSway = Vector2.SmoothDamp(_handSway, Vector2.zero, ref _handSwayDampVel, 0.2f);
        }
        else
        {
            // if tool sway is disabled, reset tool sway parameters
            _handSway = Vector3.zero;
            _handSwayDampVel = Vector3.zero;
        }
    }

    private void UpdateBreathingAnim()
    {
        if (Config.EnableBreathingAnim && Config.BreathingAnimStrength != 0f)
        {
            if (Time.deltaTime != 0f)
                _breathingAnimPos = Vector3.SmoothDamp(_breathingAnimPos, _breathingAnimTargetPos, ref _breathingAnimDampVel, 1f);

            AddToolOffsets(Config.BreathingAnimStrength * 0.005f * _breathingAnimPos);

            if (Time.time >= _breathingAnimNextUpdateTime)
            {
                // choose random tool offset
                _breathingAnimTargetPos = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                _breathingAnimNextUpdateTime = Time.time + Random.Range(0.1f, 1f);
            }
        }
        else
        {
            _breathingAnimPos = Vector3.zero;
            _breathingAnimTargetPos = Vector3.zero;
            _breathingAnimDampVel = Vector3.zero;
            _breathingAnimNextUpdateTime = 0f;
        }
    }

    private void UpdateScoutAnim()
    {
        if (Config.EnableScoutAnim)
        {
            if (_isScoutAnimActive)
            {
                if (Time.deltaTime != 0f)
                {
                    float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
                    // damp moves quickly during the initial recoil, and slowly during the recovery
                    float dampTime = targetRecoil > _scoutAnimStrength ? 0.05f : 0.1f;
                    _scoutAnimStrength = Mathf.SmoothDamp(_scoutAnimStrength, targetRecoil, ref _scoutAnimVel, dampTime);
                }

                if (_scoutAnimStrength != 0f)
                {
                    // apply recoils to camera and scout launcher
                    _cameraOffsetRoot.AddOffset(Quaternion.Euler(_scoutAnimStrength * new Vector3(-5f, 0f, -5f)));
                    _probeLauncherOffsetRoot.AddOffset(new Vector3(0.25f, -0.25f, -0.5f) * _scoutAnimStrength, Quaternion.Euler(new Vector3(-15f, 0f, -15f) * _scoutAnimStrength));
                }
                else
                {
                    _isScoutAnimActive = false;
                }
            }
        }
        else
        {
            // reset recoil parameters if disabled
            _isScoutAnimActive = false;
            _scoutAnimStrength = 0f;
            _scoutAnimVel = 0f;
        }
    }

    private void UpdateLandingAnim()
    {
        if (Config.EnableLandingAnim)
        {
            if (Time.deltaTime != 0f)
            {
                if (_isLandingAnimActive)
                {
                    // update camera height based on landing speed
                    float playerScale = ModMain.SmolHatchlingAPI != null ? ModMain.SmolHatchlingAPI.GetPlayerScale() : 1f;
                    _landingAnimPos = Mathf.Min(_landingAnimPos - _lastLandedSpeed * playerScale * Time.deltaTime, 0f);
                    if (_landingAnimPos <= -0.25f)
                    {
                        // landing anim bottoms out at -0.25
                        _landingAnimPos = -0.25f;
                        _isLandingAnimActive = false;
                    }
                }
                else
                {
                    _landingAnimPos = Mathf.SmoothDamp(_landingAnimPos, 0f, ref _landingAnimDampVel, 0.2f);
                }
            }

            // apply offset
            _cameraOffsetRoot.AddOffset(new Vector3(0f, _landingAnimPos, 0f));

            // keep track of player velocity
            _lastPlayerVel = _playerController.GetAttachedOWRigidbody().GetVelocity();
        }
        else
        {
            // reset landing anim parameters if feature is disabled
            _lastPlayerVel = Vector3.zero;
            _landingAnimPos = 0f;
            _landingAnimDampVel = 0f;
            _isLandingAnimActive = false;
        }
    }

    private void UpdateSprintAnim()
    {
        if (Config.EnableSprintingAnim && ModMain.HikersModAPI != null)
        {
            if (Time.deltaTime != 0f)
                _sprintAnimScale = Mathf.SmoothDamp(_sprintAnimScale, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _sprintAnimDampVel, 0.2f);
            AddToolOffsets(Quaternion.Euler(15f * _sprintAnimScale, 0f, 0f));
        }
        else
        {
            _sprintAnimScale = 0f;
            _sprintAnimDampVel = 0f;
        }
    }

    private void UpdateHideStowedItems()
    {
        var itemCarryTool = _toolModeSwapper.GetItemCarryTool();
        if (Config.HideStowedItems && itemCarryTool._heldItem != null && !itemCarryTool.IsPuttingAway() && _toolModeSwapper.GetToolMode() != ToolMode.Item)
            _itemToolOffsetRoot.AddOffset(Vector3.back);
    }

    private void Update()
    {
        UpdateViewbob();
        UpdateHandHeightOffset();
        UpdateHandSway();
        UpdateBreathingAnim();
        UpdateScoutAnim();
        UpdateLandingAnim();
        UpdateSprintAnim();
        UpdateHideStowedItems();
    }
}
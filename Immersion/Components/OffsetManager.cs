using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class OffsetManager : MonoBehaviour
{
    private PlayerCharacterController _playerController;
    
    private PlayerAnimController _animController;

    private PlayerCameraController _cameraController;

    private ToolModeSwapper _toolModeSwapper;

    private OffsetController _cameraOffsetter;

    private OffsetController _itemToolOffsetter;

    private OffsetController _signalscopeOffsetter;

    private OffsetController _probeLauncherOffsetter;

    private OffsetController _translatorOffsetter;

    private float _viewbobTime;

    private float _viewbobScale;

    private float _viewbobDampVel;

    private Vector2 _toolSway;

    private Vector2 _toolSwayDampVel;

    private Vector3 _breathingAnimPos;

    private Vector3 _breathingAnimTargetPos;

    private Vector3 _breathingAnimDampVel;

    private float _breathingAnimNextUpdateTime;

    private float _lastScoutLaunchTime;

    private float _scoutRecoil;

    private float _scoutRecoilVel;

    private Vector3 _lastPlayerVel;

    private bool _isLandingAnimActive;

    private float _lastLandedSpeed;

    private float _landingAnimPos;

    private float _landingAnimDampVel;

    private float _sprintAnimScale;

    private float _sprintAnimDampVel;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void PlayerCameraController_Start_Postfix(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<OffsetManager>();
        __instance._playerCamera.nearClipPlane = ModMain.Instance.FixItemClipping ? 0.05f : 0.1f;
    }

    private void Awake()
    {
        // get references to required components
        _playerController = Locator.GetPlayerController();
        _animController = _playerController.GetComponentInChildren<PlayerAnimController>();
        _cameraController = Locator.GetPlayerCameraController();
        _toolModeSwapper = Locator.GetToolModeSwapper();

        // create offsetters
        _cameraOffsetter = _cameraController.gameObject.AddComponent<OffsetController>();
        var mainCamera = _cameraController._playerCamera.mainCamera.transform;
        _itemToolOffsetter = mainCamera.Find("ItemCarryTool").gameObject.AddComponent<OffsetController>();
        _signalscopeOffsetter = mainCamera.Find("Signalscope").gameObject.AddComponent<OffsetController>();
        _probeLauncherOffsetter = mainCamera.Find("ProbeLauncher").gameObject.AddComponent<OffsetController>();
        _translatorOffsetter = mainCamera.Find("NomaiTranslatorProp").gameObject.AddComponent<OffsetController>();

        _playerController.OnBecomeGrounded += () =>
        {
            // if the player lands with a downward speed of at least 5, play landing anim
            Vector3 landingVel = (_lastPlayerVel - _playerController.GetGroundBody().GetPointVelocity(_playerController.GetGroundContactPoint()));
            float landingSpeed = -_playerController.transform.InverseTransformVector(landingVel).y;

            if (ModMain.Instance.SmolHatchlingAPI != null)
            {
                // avoid dividing by 0
                float playerScale = ModMain.Instance.SmolHatchlingAPI.GetPlayerScale();
                if (playerScale != 0f)
                    landingSpeed /= ModMain.Instance.SmolHatchlingAPI.GetPlayerScale();
            }

            if (landingSpeed >= 5f)
            {
                _lastLandedSpeed = landingSpeed;
                _isLandingAnimActive = true;
            }
        };

        _probeLauncherOffsetter.GetComponent<ProbeLauncher>().OnLaunchProbe += (_) =>
        {
            // play scout launcher animation if enabled
            if (ModMain.Instance.EnableScoutAnim)
                _lastScoutLaunchTime = Time.time;
        };
    }

    private void AddToolOffsets(Vector3 position)
    {
        // apply different scaling factors for different tools
        _itemToolOffsetter.AddOffset(position);
        _signalscopeOffsetter.AddOffset(position);
        _probeLauncherOffsetter.AddOffset(3f * position);
        _translatorOffsetter.AddOffset(3f * position);
    }

    private void AddToolOffsets(Quaternion rotation)
    {
        // apply same rotation offset for all tools
        _itemToolOffsetter.AddOffset(rotation);
        _signalscopeOffsetter.AddOffset(rotation);
        _probeLauncherOffsetter.AddOffset(rotation);
        _translatorOffsetter.AddOffset(rotation);
    }

    private void AddToolOffsets(Vector3 position, Quaternion rotation)
    {
        AddToolOffsets(position);
        AddToolOffsets(rotation);
    }

    private void UpdateViewbob()
    {
        // only do this if player is not movement locked and viewbob is enabled for camera or tool
        if (!_playerController._isMovementLocked && (ModMain.Instance.EnableHeadBob || ModMain.Instance.EnableHandBob))
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

                    if (ModMain.Instance.SmolHatchlingAPI != null)
                    {
                        // avoid dividing by 0
                        float playerScale = ModMain.Instance.SmolHatchlingAPI.GetPlayerScale();
                        if (playerScale != 0f)
                            groundVel /= ModMain.Instance.SmolHatchlingAPI.GetPlayerScale();
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
            if (ModMain.Instance.EnableHeadBob)
                _cameraOffsetter.AddOffset(ModMain.Instance.HeadBobStrength * 0.02f * new Vector3(viewBob.x, viewBob.y));

            // apply tool offset if tool bob is enabled
            if (ModMain.Instance.EnableHandBob)
            {
                var offsetPos = ModMain.Instance.HandBobStrength * new Vector3(0.02f * viewBob.x, 0.003f * viewBob.y);
                var offsetRot = Quaternion.Euler(ModMain.Instance.HandBobStrength * _viewbobScale * -0.75f * Mathf.Sin(_viewbobTime * 4f * Mathf.PI), 0f, 0f);
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

    private void UpdateDynamicToolPos()
    {
        // only do this if dynamic tool position is enabled and strength is non-zero
        if (ModMain.Instance.EnableHandHeightOffset)
        {
            float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
            Vector3 toolOffset = Vector3.zero;
            // trig is used for circular motion
            // tool moves down+back when looking up, and up+back when looking down
            // tool is not offset when looking straight ahead
            toolOffset.z = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1;
            toolOffset.y = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            AddToolOffsets(ModMain.Instance.HandHeightOffsetStrength * 0.05f * toolOffset);
        }
    }

    private void UpdateToolSway()
    {
        // only do this if tool sway is enabled
        if (ModMain.Instance.EnableHandSway)
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
                _toolSway -= lookInput * (1f - Mathf.Min((_toolSway - lookInput).magnitude, 1));
            }

            // x sway is less pronounced the more up/down the player is looking
            // sway is split into local (relative to camera) and global (relative to player)
            float xSwayScale = (Mathf.Cos(degreesY / 90f * Mathf.PI) + 1f) * 0.5f;
            float localZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.y) - 1f);
            float globalZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.x) - 1f);

            // calculate and apply the final offset
            var offset = new Vector3(_toolSway.x * xSwayScale, _toolSway.y, localZOffset);
            offset += xSwayScale * globalZOffset * _cameraController.transform.InverseTransformDirection(_playerController.transform.forward);
            offset *= ModMain.Instance.HandSwayStrength * 0.25f;
            AddToolOffsets(offset);

            // decay tool sway
            _toolSway = Vector2.SmoothDamp(_toolSway, Vector2.zero, ref _toolSwayDampVel, 0.2f);
        }
        else
        {
            // if tool sway is disabled, reset tool sway parameters
            _toolSway = Vector3.zero;
            _toolSwayDampVel = Vector3.zero;
        }
    }

    private void UpdateBreathingAnim()
    {
        if (ModMain.Instance.EnableBreathingAnim && ModMain.Instance.BreathingAnimStrength != 0f)
        {
            if (Time.deltaTime != 0f)
                _breathingAnimPos = Vector3.SmoothDamp(_breathingAnimPos, _breathingAnimTargetPos, ref _breathingAnimDampVel, 1f);

            AddToolOffsets(ModMain.Instance.BreathingAnimStrength * 0.005f * _breathingAnimPos);

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
        if (ModMain.Instance.EnableScoutAnim)
        {
            if (Time.deltaTime != 0f)
            {
                float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
                // damp moves quickly during the initial recoil, and slowly during the recovery
                float dampTime = targetRecoil > _scoutRecoil ? 0.05f : 0.1f;
                _scoutRecoil = Mathf.SmoothDamp(_scoutRecoil, targetRecoil, ref _scoutRecoilVel, dampTime);
            }

            // apply recoils to camera and scout launcher
            _cameraOffsetter.AddOffset(Quaternion.Euler(_scoutRecoil * new Vector3(-5f, 0f, -5f)));
            _probeLauncherOffsetter.AddOffset(new Vector3(0.25f, -0.25f, -0.5f) * _scoutRecoil, Quaternion.Euler(new Vector3(-15f, 0f, -15f) * _scoutRecoil));
        }
        else
        {
            // reset recoil parameters if disabled
            _scoutRecoil = 0f;
            _scoutRecoilVel = 0f;
        }
    }

    private void UpdateLandingAnim()
    {
        if (ModMain.Instance.EnableLandingAnim)
        {
            if (Time.deltaTime != 0f)
            {
                if (_isLandingAnimActive)
                {
                    // update camera height based on landing speed



                    float playerScale = ModMain.Instance.SmolHatchlingAPI != null ? ModMain.Instance.SmolHatchlingAPI.GetPlayerScale() : 1f;
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
            _cameraOffsetter.AddOffset(new Vector3(0f, _landingAnimPos, 0f));

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
        if (ModMain.Instance.EnableSprintingAnim && ModMain.Instance.HikersModAPI != null)
        {
            if (Time.deltaTime != 0f)
                _sprintAnimScale = Mathf.SmoothDamp(_sprintAnimScale, ModMain.Instance.HikersModAPI.IsSprinting() ? 1f : 0f, ref _sprintAnimDampVel, 0.2f);
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
        if (ModMain.Instance.HideStowedItems && itemCarryTool._heldItem != null && !itemCarryTool.IsPuttingAway() && _toolModeSwapper.GetToolMode() != ToolMode.Item)
            _itemToolOffsetter.AddOffset(Vector3.back);
    }

    private void LateUpdate()
    {
        UpdateViewbob();
        UpdateDynamicToolPos();
        UpdateToolSway();
        UpdateBreathingAnim();
        UpdateScoutAnim();
        UpdateLandingAnim();
        UpdateSprintAnim();
        UpdateHideStowedItems();
    }
}
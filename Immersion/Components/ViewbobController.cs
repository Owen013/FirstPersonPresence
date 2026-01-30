using UnityEngine;

namespace Immersion.Components;

public class ViewbobController : MonoBehaviour
{
    private PlayerCharacterController _playerController;
    
    private PlayerAnimController _animController;

    private PlayerCameraController _cameraController;

    private OffsetController _cameraOffsetter;

    private OffsetController _itemToolOffsetter;

    private OffsetController _signalscopeOffsetter;

    private OffsetController _probeLauncherOffsetter;

    private OffsetController _translatorOffsetter;

    private float _viewbobTime;

    private float _viewbobStrength;

    private Vector3 _toolSway;

    private Vector3 _toolSwayDampVelocity;

    private Vector3 _lastPlayerVelocity;

    private bool _isLandingAnimActive;

    private float _landingAnimPos;

    private float _landingAnimVelocity;

    private void StartLandingAnim(float landingSpeed)
    {
        _landingAnimVelocity = -landingSpeed;
        _isLandingAnimActive = true;
    }

    private void Awake()
    {
        // get references to required components
        _playerController = Locator.GetPlayerController();
        _animController = _playerController.GetComponentInChildren<PlayerAnimController>();
        _cameraController = GetComponent<PlayerCameraController>();
        _cameraOffsetter = _cameraController.gameObject.AddComponent<OffsetController>();

        // create PlayerTool offsetters
        var mainCamera = _cameraController._playerCamera.mainCamera.transform;
        _itemToolOffsetter = mainCamera.Find("ItemCarryTool").gameObject.AddComponent<OffsetController>();
        _signalscopeOffsetter = mainCamera.Find("Signalscope").gameObject.AddComponent<OffsetController>();
        _probeLauncherOffsetter = mainCamera.Find("ProbeLauncher").gameObject.AddComponent<OffsetController>();
        _translatorOffsetter = mainCamera.Find("NomaiTranslatorProp").gameObject.AddComponent<OffsetController>();

        _playerController.OnBecomeGrounded += () =>
        {
            if (_viewbobStrength < 0.1f)
                _viewbobTime = 0f;

            float landingSpeed = (_lastPlayerVelocity - _playerController.GetGroundBody().GetPointVelocity(_playerController.GetGroundContactPoint())).magnitude;
            StartLandingAnim(landingSpeed);
        };
    }

    private void ApplyToolOffsets(Vector3 position)
    {
        // apply different scaling factors for different tools
        _itemToolOffsetter.AddOffset(position);
        _signalscopeOffsetter.AddOffset(position);
        _probeLauncherOffsetter.AddOffset(3f * position);
        _translatorOffsetter.AddOffset(3f * position);
    }

    private void ApplyToolOffsets(Quaternion rotation)
    {
        // apply same rotation offset for all tools
        _itemToolOffsetter.AddOffset(rotation);
        _signalscopeOffsetter.AddOffset(rotation);
        _probeLauncherOffsetter.AddOffset(rotation);
        _translatorOffsetter.AddOffset(rotation);
    }

    private void ApplyToolOffsets(Vector3 position, Quaternion rotation)
    {
        ApplyToolOffsets(position);
        ApplyToolOffsets(rotation);
    }

    private void UpdateViewbob()
    {
        bool isCameraBobEnabled = ModMain.Instance.EnableCameraBob && ModMain.Instance.CameraBobStrength != 0f;
        bool isToolBobEnabled = ModMain.Instance.EnableToolBob && ModMain.Instance.ToolBobStrength != 0f;
        // only do this if viewbob is enabled for camera or tool
        if (isCameraBobEnabled || isToolBobEnabled)
        {
            // viewbob cycle increases based on player ground speed
            // viewbob time and viewbob strength are used by both camera and tool bobbing
            _viewbobTime += _animController._animator.speed * Time.deltaTime;
            if (_playerController.IsGrounded())
            {
                // change viewbob strength quickly if on ground
                Vector3 groundVelocity = _playerController.GetRelativeGroundVelocity();
                groundVelocity.y = 0f;
                _viewbobStrength = Mathf.MoveTowards(_viewbobStrength, Mathf.Min(groundVelocity.magnitude / 6f, 2f), 5f * Time.deltaTime);
            }
            else
            {
                // decay viewbob strength slowly if in air
                _viewbobStrength = Mathf.MoveTowards(_viewbobStrength, 0f, Time.deltaTime);
            }

            // trig is used for a circular viewbob motion
            var viewBob = _viewbobStrength * new Vector2(Mathf.Sin(_viewbobTime * 2f * Mathf.PI), Mathf.Cos(_viewbobTime * 4f * Mathf.PI));

            // apply camera offset if camera bob is enabled
            if (isCameraBobEnabled)
                _cameraOffsetter.AddOffset(ModMain.Instance.CameraBobStrength * 0.02f * new Vector3(viewBob.x, viewBob.y));

            // apply tool offset if tool bob is enabled
            if (isToolBobEnabled)
            {
                var offsetPos = ModMain.Instance.ToolBobStrength * new Vector3(0.02f * viewBob.x, 0.003f * viewBob.y);
                var offsetRot = Quaternion.Euler(ModMain.Instance.ToolBobStrength * _viewbobStrength * -1f * Mathf.Sin(_viewbobTime * 4f * Mathf.PI), 0f, 0f);
                ApplyToolOffsets(offsetPos, offsetRot);
            }
        }
        // reset viewbob parameters if both camera and tool bob are disabled
        else
        {
            _viewbobTime = 0f;
            _viewbobStrength = 0f;
        }
    }

    private void UpdateDynamicToolPos()
    {
        // only do this if dynamic tool position is enabled and strength is non-zero
        if (ModMain.Instance.EnableDynamicToolPos && ModMain.Instance.DynamicToolPosStrength != 0f)
        {
            float verticalLookAmount = _cameraController.GetDegreesY() / 90f;
            Vector3 toolOffset = Vector3.zero;
            // trig is used for circular motion
            // tool moves down+back when looking up, and up+back when looking down
            // tool is not offset when looking straight ahead
            toolOffset.z = Mathf.Cos(verticalLookAmount * Mathf.PI / 3f) - 1;
            toolOffset.y = -Mathf.Sin(verticalLookAmount * Mathf.PI / 3f);
            ApplyToolOffsets(ModMain.Instance.DynamicToolPosStrength * 0.05f * toolOffset);
        }
    }

    private void UpdateToolSway()
    {
        // only do this if tool sway is enabled
        if (ModMain.Instance.EnableToolSway)
        {
            Vector2 lookDelta = OWInput.GetAxisValue(InputLibrary.look);
            float degreesY = _cameraController.GetDegreesY();
            if (degreesY >= PlayerCameraController._maxDegreesYNormal)
                lookDelta.y = Mathf.Min(0f, lookDelta.y);
            if (degreesY <= PlayerCameraController._minDegreesYNormal)
                lookDelta.y = Mathf.Max(0f, lookDelta.y);

            var newSway = new Vector3(-lookDelta.x * Mathf.Cos(degreesY * Mathf.PI / 180f), -lookDelta.y, 0f);
            newSway /= _toolSway.magnitude + 1f;
            _toolSway += newSway;
            ApplyToolOffsets(ModMain.Instance.ToolSwayStrength * 0.05f * _toolSway);
        }
        else
        {
            _toolSway = Vector3.zero;
            _toolSwayDampVelocity = Vector3.zero;
        }
    }

    private void UpdateLandingAnim()
    {
        _landingAnimPos = Mathf.Min(_landingAnimPos + _landingAnimVelocity * Time.deltaTime, 0f);

        if (_isLandingAnimActive)
        {
            if (_landingAnimPos >= 0f && _landingAnimVelocity >= 0f)
            {
                _landingAnimPos = 0f;
                _landingAnimVelocity = 0f;
                _isLandingAnimActive = false;
                return;
            }
            else if (_landingAnimPos <= -0.5f && _landingAnimVelocity < 0f)
            {
                _landingAnimPos = -0.5f;
                _landingAnimVelocity = 0f;
            }
            else
            {
                _landingAnimVelocity = Mathf.MoveTowards(_landingAnimVelocity, 1f, -_landingAnimPos * 20f * Time.deltaTime);
            }

            _cameraOffsetter.AddOffset(new Vector3(0f, _landingAnimPos, 0f));
        }
    }

    private void Update()
    {
        UpdateViewbob();
        UpdateDynamicToolPos();
        UpdateToolSway();
        //UpdateLandingAnim();
    }

    private void FixedUpdate()
    {
        // decay tool sway
        _toolSway = Vector3.SmoothDamp(_toolSway, Vector3.zero, ref _toolSwayDampVelocity, 0.5f);
        _lastPlayerVelocity = _playerController.GetAttachedOWRigidbody().GetVelocity();
    }
}
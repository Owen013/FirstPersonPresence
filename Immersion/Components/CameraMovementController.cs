using UnityEngine;

namespace Immersion.Components;

public class CameraMovementController : MonoBehaviour
{
    public static CameraMovementController Instance { get; private set; }
    public GameObject CameraRoot { get; private set; }
    public GameObject ToolRoot { get; private set; }
    public GameObject BigToolRoot { get; private set; }

    private PlayerCameraController _cameraController;
    private PlayerAnimController _animController;
    private PlayerCharacterController _characterController;
    private float _viewBobTime;
    private float _viewBobIntensity;
    private float _viewBobVelocity;
    private float _lastLandedTime;
    private float _lastScoutLaunchTime;
    private float _scoutRecoil;
    private float _scoutRecoilVelocity;
    private Vector3 _toolSway;
    private Vector3 _toolSwayVelocity;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cameraController = GetComponent<PlayerCameraController>();
        _animController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>();
        _characterController = Locator.GetPlayerController();

        // create view bob root and parent camera to it
        CameraRoot = new();
        CameraRoot.name = "CameraRoot";
        CameraRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform.parent;
        CameraRoot.transform.localPosition = Vector3.zero;
        CameraRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.parent = CameraRoot.transform;

        // create tool root and parent tools to it
        ToolRoot = new();
        ToolRoot.name = "ToolRoot";
        ToolRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        ToolRoot.transform.localPosition = Vector3.zero;
        ToolRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ItemCarryTool").transform.parent = ToolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("Signalscope").transform.parent = ToolRoot.transform;

        // create a separate root for the scout launcher since it's a lot bigger and farther from the camera
        BigToolRoot = new();
        BigToolRoot.name = "BigToolRoot";
        BigToolRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        BigToolRoot.transform.localPosition = Vector3.zero;
        BigToolRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ProbeLauncher").transform.parent = BigToolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("NomaiTranslatorProp").transform.parent = BigToolRoot.transform;

        // subscribe to events
        _characterController.OnBecomeGrounded += () =>
        {
            _lastLandedTime = Time.time;
        };
        _characterController.GetComponentInChildren<PlayerProbeLauncher>().OnLaunchProbe += (probe) =>
        {
            if (Config.UseScoutAnim)
            {
                _lastScoutLaunchTime = Time.time;
            }
        };
    }

    private void Update()
    {
        // step viewbob time based on how fast the animator is going and wrap between 0 and 1
        _viewBobTime = Mathf.Repeat(_viewBobTime + 1.033333f * _animController._animator.speed * Time.deltaTime, 1f);

        if (!_characterController.IsGrounded() && !_characterController._isMovementLocked)
        {
            // if in midair, use falling and/or jumping animation
            float fallFraction = Config.UseFallAnim ? _animController._animator.GetFloat("FreefallSpeed") : 0f;
            float jumpFraction = Config.UseJumpAnim ? Mathf.Max((_characterController._lastJumpTime + 0.5f - Time.time) * 2f, 0f) : 0f;
            _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(fallFraction + jumpFraction, 1f) * 0.075f, ref _viewBobVelocity, 0.075f);
        }
        else
        {
            // if on ground, use walking and/or landing animation
            float walkFraction = Mathf.Sqrt(Mathf.Pow(_animController._animator.GetFloat("RunSpeedX"), 2f) + Mathf.Pow(_animController._animator.GetFloat("RunSpeedY"), 2f));
            float landingFraction = Config.UseLandingAnim ? Mathf.Max((_lastLandedTime + 0.25f - Time.time) * 6f, 0f) : 0f;
            _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(walkFraction + landingFraction, 5f) * 0.02f, ref _viewBobVelocity, 0.075f);
        }

        // camera bob
        float bobX = Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity;
        float bobY = Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity;
        // scale camera bob if Smol Hatchling is installed
        if (Main.Instance.SmolHatchlingAPI != null)
        {
            bobX *= Main.Instance.SmolHatchlingAPI.GetCurrentScale().x;
            bobY *= Main.Instance.SmolHatchlingAPI.GetCurrentScale().y;
        }
        CameraRoot.transform.localPosition = new Vector3(bobX * Config.ViewBobXAmount, bobY * Config.ViewBobYAmount, 0f);
        CameraRoot.transform.localRotation = Quaternion.Euler(new Vector3(bobY * 5f * Config.ViewBobPitchAmount, 0f, bobX * 5f * Config.ViewBobRollAmount));

        // tool bob
        float toolBobX = Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity * Config.ToolBobXAmount * 0.25f;
        float toolBobY = Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity * Config.ToolBobYAmount * 0.25f;
        float toolBobZ = -Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity * Config.ToolBobZAmount * 0.25f;
        ToolRoot.transform.localPosition = new Vector3(toolBobX, toolBobY, toolBobZ);
        ToolRoot.transform.localRotation = Quaternion.Euler(new Vector3(bobY * 25f * Config.ToolBobPitchAmount, 0f, bobX * 25f * Config.ToolBobRollAmount));

        if (Config.ToolHeightYAmount != 0f || Config.ToolHeightZAmount != 0f)
        {
            UpdateDynamicToolHeight();
        }
        if (Config.ToolSwaySensitivity != 0f || _toolSway != Vector3.zero)
        {
            UpdateToolSway();
        }

        // big tool root position offset needs to be 3x bigger because the tools in it are further away and appear to move less
        BigToolRoot.transform.localPosition = ToolRoot.transform.localPosition * 3f;
        BigToolRoot.transform.localRotation = ToolRoot.transform.localRotation;
        
        // do this after setting the big tool position as it only applyies to big tool root
        if (Config.UseScoutAnim)
        {
            UpdateScoutAnim();
        }
    }

    private void UpdateDynamicToolHeight()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolHeight;

        if (Config.ToolHeightBehavior == "Legacy")
        {
            dynamicToolHeight = new Vector3(0f, -degreesY * 0.02222f * Config.ToolHeightYAmount, (Mathf.Cos(degreesY * 0.03490f) - 1) * 0.3f * Config.ToolHeightZAmount) * 0.04f;
        }
        else
        {
            dynamicToolHeight = new Vector3(0f, -degreesY * 0.02222f * Config.ToolHeightYAmount, -degreesY * 0.01111f * Config.ToolHeightZAmount) * 0.04f;
        }
        
        ToolRoot.transform.localPosition += dynamicToolHeight;
    }

    private void UpdateToolSway()
    {
        // get look input if player is in normal movement mode
        Vector2 lookDelta;
        if (!OWInput.IsInputMode(InputMode.Character) || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
        {
            lookDelta = Vector2.zero;
        }
        else
        {
            lookDelta = OWInput.GetAxisValue(InputLibrary.look) * 0.005f * Config.ToolSwaySensitivity;
        }

        float degreesY = _cameraController.GetDegreesY();
        // decrease horizontal sway the further up or down the player is looking
        lookDelta.x *= (Mathf.Cos(degreesY * 0.03490f) + 1f) * 0.5f;
        // cancel out vertical sway if the player can't turn anymore in that direction
        if ((lookDelta.y > 0f && degreesY >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0f && degreesY <= PlayerCameraController._minDegreesYNormal))
        {
            lookDelta.y = 0f;
        }

        // decay already existing tool sway and then add new tool sway
        _toolSway = Vector3.SmoothDamp(_toolSway, Vector3.zero, ref _toolSwayVelocity, 0.2f * Config.ToolSwaySmoothing);
        _toolSway += new Vector3(-lookDelta.x, -lookDelta.y, 0f) * (0.25f - _toolSway.magnitude) / 0.25f;
        // move tool backward the further it is from the default position to make tool sway move in a circular motion
        _toolSway.z = Mathf.Cos(_toolSway.magnitude * 1.57080f) - 1f;

        ToolRoot.transform.localPosition += _toolSway;
    }

    private void UpdateScoutAnim()
    {
        // plays a recoil animation for 0.5 seconds after scout launch
        float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
        float dampTime = targetRecoil > _scoutRecoil ? 0.05f : 0.1f;
        _scoutRecoil = Mathf.SmoothDamp(_scoutRecoil, targetRecoil, ref _scoutRecoilVelocity, dampTime);
        CameraRoot.transform.localPosition += new Vector3(0f, 0f, 0.15f) * _scoutRecoil;
        CameraRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-10f, 0f, -5f) * _scoutRecoil);
        BigToolRoot.transform.localPosition += new Vector3(0.5f, 0.25f, -0.5f) * _scoutRecoil;
        BigToolRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-10f, 0f, -20f) * _scoutRecoil);
    }
}
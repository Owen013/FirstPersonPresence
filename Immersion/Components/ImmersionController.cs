using HarmonyLib;
using System;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ImmersionController : MonoBehaviour
{
    public static ImmersionController Instance { get; private set; }

    public GameObject CameraRoot { get; private set; }

    public GameObject ToolRoot { get; private set; }

    public GameObject ProbeLauncherRoot { get; private set; }

    public GameObject TranslatorRoot { get; private set; }

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

    private Vector2 _toolSway;

    private Vector2 _toolSwayVelocity;

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
        CameraRoot = new GameObject("CameraRoot");
        CameraRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform.parent;
        CameraRoot.transform.localPosition = Vector3.zero;
        CameraRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.parent = CameraRoot.transform;
        CameraRoot.transform.localScale = Vector3.one;
        _cameraController._playerCamera.mainCamera.transform.localScale = Vector3.one;

        // create tool root and parent tools to it
        ToolRoot = new GameObject("ToolRoot");
        ToolRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        ToolRoot.transform.localPosition = Vector3.zero;
        ToolRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ItemCarryTool").transform.parent = ToolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("Signalscope").transform.parent = ToolRoot.transform;

        // create a separate root for the scout launcher since it's a lot bigger and farther from the camera
        ProbeLauncherRoot = new GameObject("ProbeLauncherRoot");
        ProbeLauncherRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        ProbeLauncherRoot.transform.localPosition = Vector3.zero;
        ProbeLauncherRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ProbeLauncher").transform.parent = ProbeLauncherRoot.transform;

        // create a separate root for the translator tool since it doesn't bob forward and backward
        TranslatorRoot = new GameObject("TranslatorRoot");
        TranslatorRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        TranslatorRoot.transform.localPosition = Vector3.zero;
        TranslatorRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("NomaiTranslatorProp").transform.parent = TranslatorRoot.transform;

        // subscribe to events
        Config.OnConfigure += CheckAndSetLeftyMode;
        _characterController.OnBecomeGrounded += () =>
        {
            _lastLandedTime = Time.time;
        };
        _characterController.GetComponentInChildren<PlayerProbeLauncher>().OnLaunchProbe += (probe) =>
        {
            if (Config.IsScoutAnimEnabled)
            {
                _lastScoutLaunchTime = Time.time;
            }
        };

        CheckAndSetLeftyMode();
    }

    private void OnDestroy()
    {
        Config.OnConfigure -= CheckAndSetLeftyMode;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        CameraRoot.transform.localPosition = Vector3.zero;
        CameraRoot.transform.localRotation = Quaternion.identity;
        ToolRoot.transform.localPosition = Vector3.zero;
        ToolRoot.transform.localRotation = Quaternion.identity;

        Vector3 toolBob = Vector3.zero;
        if (Config.IsViewBobEnabled || Config.IsToolBobEnabled)
        {
            float predictedViewBobTime = _viewBobTime + 1.033333f * _animController._animator.speed * 1.03f * Time.deltaTime;
            float animatorTime = _animController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.25f;

            _viewBobTime = Mathf.Floor(animatorTime) + Mathf.Repeat(Mathf.Clamp(animatorTime, predictedViewBobTime - 0.005f, predictedViewBobTime + 0.005f), 1f);

            if (!_characterController.IsGrounded() && !_characterController._isMovementLocked)
            {
                // if in midair, use falling and/or jumping animation
                float fallFraction = Config.IsFallAnimEnabled ? _animController._animator.GetFloat("FreefallSpeed") : 0f;
                float jumpFraction = Config.IsJumpAnimEnabled ? Mathf.Max((_characterController._lastJumpTime + 0.5f - Time.time) * 2f, 0f) : 0f;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(fallFraction + jumpFraction, 1f) * 0.075f, ref _viewBobVelocity, 0.075f);
            }
            else
            {
                // if on ground, use walking and/or landing animation
                float walkFraction = Mathf.Sqrt(Mathf.Pow(_animController._animator.GetFloat("RunSpeedX"), 2f) + Mathf.Pow(_animController._animator.GetFloat("RunSpeedY"), 2f));
                float landingFraction = Config.IsLandingAnimEnabled && Time.timeSinceLevelLoad > 1f ? Mathf.Max((_lastLandedTime + 0.25f - Time.time) * 6f, 0f) : 0f;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(walkFraction + landingFraction, 5f) * 0.02f, ref _viewBobVelocity, 0.075f);
            }

            // camera bob
            if (Config.IsViewBobEnabled)
            {
                Vector2 cameraBob = new Vector2(Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity, Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity);
                CameraRoot.transform.Translate(new Vector3(cameraBob.x * Config.ViewBobXAmount, cameraBob.y * Config.ViewBobYAmount));
                RotateCamera(new Vector3(-cameraBob.y * 5f * Config.ViewBobPitchAmount, 0f, -cameraBob.x * 5f * Config.ViewBobRollAmount));
            }

            // tool bob
            if (Config.IsToolBobEnabled)
            {
                toolBob.x = Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity * 0.25f;
                toolBob.y = Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity * 0.25f;
                toolBob.z = -toolBob.x * (Config.IsLeftyModeEnabled ? -1f : 1f);
                ToolRoot.transform.localPosition = new Vector3(0, toolBob.y * Config.ToolBobYAmount);
                ToolRoot.transform.Translate(new Vector3(toolBob.x * Config.ToolBobXAmount, 0, toolBob.z * Config.ToolBobZAmount), _characterController.transform);
                ToolRoot.transform.localRotation = Quaternion.Euler(new Vector3(toolBob.y * 100f * Config.ToolBobPitchAmount, 0f, -toolBob.x * 100f * Config.ToolBobRollAmount));
            }
        }

        if (Config.IsToolSwayEnabled)
        {
            UpdateToolSway();
        }
        else
        {
            _toolSway = Vector3.zero;
            _toolSwayVelocity = Vector3.zero;
        }

        if (Config.DynamicToolPosBehavior != "Disabled")
        {
            ToolRoot.transform.localPosition += GetDynamicToolPos();
        }

        // Probe Launcher position offset needs to be 3x bigger because the tools in it are further away and appear to move less
        ProbeLauncherRoot.transform.localPosition = 3 * ToolRoot.transform.localPosition;
        ProbeLauncherRoot.transform.localRotation = ToolRoot.transform.localRotation;
        if (Config.IsScoutAnimEnabled)
        {
            ApplyScoutAnim();
        }

        // Translator offset needs to be 3x bigger, also needs to bob more in the x direction and not at all in the z direction
        TranslatorRoot.transform.localPosition = 3 * ToolRoot.transform.localPosition;
        TranslatorRoot.transform.Translate(new Vector3(1.82f * toolBob.x, 0, -3 * toolBob.z), _characterController.transform);
        TranslatorRoot.transform.localRotation = ToolRoot.transform.localRotation;

        if (Config.IsHideStowedItemsEnabled)
        {
            ItemTool itemTool = Locator.GetToolModeSwapper()._itemCarryTool;
            if (!itemTool.IsEquipped() && !itemTool.IsPuttingAway())
            {
                itemTool.transform.localRotation = Quaternion.RotateTowards(itemTool.transform.localRotation, Quaternion.Euler(180, 0, 0), 180 * Time.deltaTime);
            }
        }
    }

    private void RotateCamera(Vector3 eulers)
    {
        CameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.right), eulers.x);
        CameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.up), eulers.y);
        CameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.forward), eulers.z);
    }

    private void UpdateToolSway()
    {
        // get look input if player is in normal movement mode
        Vector2 lookDelta = Vector2.zero;
        if (OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()) && Time.timeScale != 0f)
        {
            lookDelta = OWInput.GetAxisValue(InputLibrary.look) * _characterController._playerCam.fieldOfView / _characterController._initFOV * 0.01f * Time.deltaTime / Time.timeScale;
            bool isAlarming = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            lookDelta *= _characterController._signalscopeZoom || isAlarming ? PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR : PlayerCameraController.LOOK_RATE;
        }

        float degreesY = _cameraController.GetDegreesY();
        // cancel out vertical sway if the player can't turn anymore in that direction
        if ((lookDelta.y > 0f && degreesY >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0f && degreesY <= PlayerCameraController._minDegreesYNormal))
        {
            lookDelta.y = 0f;
        }

        // decay already existing tool sway and then add new tool sway
        _toolSway = Vector2.SmoothDamp(_toolSway, Vector2.zero, ref _toolSwayVelocity, 0.2f * Config.ToolSwaySmoothing, 5f);
        _toolSway = Vector2.ClampMagnitude(_toolSway - lookDelta * (1 - _toolSway.magnitude), 1);

        float localZOffset = Mathf.Cos(Mathf.PI * 0.5f * Mathf.Abs(_toolSway.y) * 2f) - 1f;
        float globalZOffset = Mathf.Cos(Mathf.PI * 0.5f * Mathf.Abs(_toolSway.x) * 2f) - 1f;
        float xSwayMultiplier = (Mathf.Cos(degreesY * 0.03490f) + 1f) * 0.5f;
        ToolRoot.transform.localPosition += 0.15f * Config.ToolSwayTranslateAmount * new Vector3(0, _toolSway.y, 0.25f * localZOffset);
        ToolRoot.transform.Translate(0.15f * xSwayMultiplier * Config.ToolSwayTranslateAmount * new Vector3(_toolSway.x, 0, 0.25f * globalZOffset), _characterController.transform);
        ToolRoot.transform.localRotation *= Quaternion.Euler(-30 * Config.ToolSwayRotateAmount * new Vector3(_toolSway.y, 0, 0));
        ToolRoot.transform.RotateAround(_characterController.transform.position, _characterController._owRigidbody.GetLocalUpDirection(), 30 * Config.ToolSwayRotateAmount * _toolSway.x);
    }

    private Vector3 GetDynamicToolPos()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolPos;
        if (Config.DynamicToolPosBehavior == "Legacy")
        {
            // new behavior moves tool closer to camera the more you are looking up/down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * Config.DynamicToolPosYAmount, -degreesY * 0.01111f * Config.DynamicToolPosZAmount) * 0.04f;
        }
        else
        {
            // legacy behavior moves tool closer when looking up and further when looking down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * Config.DynamicToolPosYAmount, (Mathf.Cos(degreesY * 0.03490f) - 1) * 0.3f * Config.DynamicToolPosZAmount) * 0.04f;
        }

        return dynamicToolPos;
    }

    // plays a recoil animation for 0.5 seconds after scout launch
    private void ApplyScoutAnim()
    {
        float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
        float dampTime = targetRecoil > _scoutRecoil ? 0.05f : 0.1f;
        _scoutRecoil = Mathf.SmoothDamp(_scoutRecoil, targetRecoil, ref _scoutRecoilVelocity, dampTime);
        CameraRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-10f, Config.IsLeftyModeEnabled ? -1f : 1f, -5f * (Config.IsLeftyModeEnabled ? -1f : 1f)) * _scoutRecoil);
        ProbeLauncherRoot.transform.localPosition += new Vector3(0.5f * (Config.IsLeftyModeEnabled ? -1f : 1f), 0.25f, -0.5f) * _scoutRecoil;
        ProbeLauncherRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-10f, 0f, -20f * (Config.IsLeftyModeEnabled ? -1f : 1f)) * _scoutRecoil);
    }

    private void CheckAndSetLeftyMode()
    {
        if (Config.IsLeftyModeEnabled)
        {
            ToolRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
            ProbeLauncherRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            ToolRoot.transform.localScale = Vector3.one;
            ProbeLauncherRoot.transform.localScale = Vector3.one;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void AddToPlayerCamera(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<ImmersionController>();
    }
}
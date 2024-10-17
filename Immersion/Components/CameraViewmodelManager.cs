using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class CameraViewmodelManager : MonoBehaviour
{
    private PlayerCameraController _cameraController;

    private PlayerAnimController _animController;

    private PlayerCharacterController _characterController;

    private GameObject _cameraRoot;

    private GameObject _mainToolRoot;

    private GameObject _probeLauncherRoot;

    private GameObject _translatorRoot;

    private float _viewBobTime;

    private float _viewBobIntensity;

    private float _viewBobVelocity;

    private float _lastLandedTime;

    private Vector2 _toolSway;

    private Vector2 _toolSwayVelocity;

    private float _lastScoutLaunchTime;

    private float _scoutRecoil;

    private float _scoutRecoilVelocity;

    private void Start()
    {
        _cameraController = GetComponent<PlayerCameraController>();
        _animController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>();
        _characterController = Locator.GetPlayerController();

        static GameObject CreateRoot(string name, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.parent = parent;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        // create view bob root and parent camera to it
        _cameraRoot = CreateRoot("CameraRoot", _cameraController._playerCamera.mainCamera.transform.parent);
        _cameraController._playerCamera.mainCamera.transform.parent = _cameraRoot.transform;

        // create tool root and parent tools to it
        _mainToolRoot = CreateRoot("MainToolRoot", _cameraController._playerCamera.mainCamera.transform);
        _cameraController._playerCamera.mainCamera.transform.Find("ItemCarryTool").transform.parent = _mainToolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("Signalscope").transform.parent = _mainToolRoot.transform;

        // create a separate root for the scout launcher since it's a lot bigger and farther from the camera
        _probeLauncherRoot = CreateRoot("ProbeLauncherRoot", _cameraController._playerCamera.mainCamera.transform);
        _cameraController._playerCamera.mainCamera.transform.Find("ProbeLauncher").transform.parent = _probeLauncherRoot.transform;

        // create a separate root for the translator tool since it doesn't bob forward and backward
        _translatorRoot = CreateRoot("TranslatorRoot", _cameraController._playerCamera.mainCamera.transform);
        _cameraController._playerCamera.mainCamera.transform.Find("NomaiTranslatorProp").transform.parent = _translatorRoot.transform;

        // subscribe to events
        _characterController.OnBecomeGrounded += () =>
        {
            _lastLandedTime = Time.time;
        };
        _characterController.GetComponentInChildren<PlayerProbeLauncher>().OnLaunchProbe += (probe) =>
        {
            if (ModMain.IsScoutAnimEnabled)
            {
                _lastScoutLaunchTime = Time.time;
            }
        };
        ModMain.Instance.OnConfigure += () =>
        {
            if (ModMain.IsLeftyModeEnabled)
            {
                _mainToolRoot.transform.localScale = new Vector3(-1, 1, 1);
                _probeLauncherRoot.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                _mainToolRoot.transform.localScale = Vector3.one;
                _probeLauncherRoot.transform.localScale = Vector3.one;
            }
        };

        if (ModMain.IsLeftyModeEnabled)
        {
            _mainToolRoot.transform.localScale = new Vector3(-1, 1, 1);
            _probeLauncherRoot.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void LateUpdate()
    {
        // reset everything
        _cameraRoot.transform.localPosition = Vector3.zero;
        _cameraRoot.transform.localRotation = Quaternion.identity;
        _mainToolRoot.transform.localPosition = Vector3.zero;
        _mainToolRoot.transform.localRotation = Quaternion.identity;

        Vector3 toolBob = Vector3.zero;
        if (ModMain.IsViewBobEnabled || ModMain.IsToolBobEnabled)
        {
            float predictedViewBobTime = _viewBobTime + 1.033333f * _animController._animator.speed * Time.deltaTime;
            float animatorTime = _animController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.25f;

            _viewBobTime = Mathf.Floor(animatorTime) + Mathf.Repeat(Mathf.Clamp(animatorTime, predictedViewBobTime - 0.3f * Time.deltaTime, predictedViewBobTime + 0.3f * Time.deltaTime), 1);

            if (!_characterController.IsGrounded() && !_characterController._isMovementLocked)
            {
                // if in midair, use falling and/or jumping animation
                float fallFraction = ModMain.IsFallAnimEnabled ? _animController._animator.GetFloat("FreefallSpeed") : 0;
                float jumpFraction = ModMain.IsJumpAnimEnabled ? Mathf.Max((_characterController._lastJumpTime + 0.5f - Time.time) * 2, 0) : 0;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(fallFraction + jumpFraction, 1) * 0.075f, ref _viewBobVelocity, 0.075f);
            }
            else
            {
                // if on ground, use walking and/or landing animation
                float walkFraction = new Vector2(_animController._animator.GetFloat("RunSpeedX"), _animController._animator.GetFloat("RunSpeedY")).magnitude;
                float landingFraction = ModMain.IsLandingAnimEnabled && Time.timeSinceLevelLoad > 1 ? Mathf.Max((_lastLandedTime + 0.25f - Time.time) * 6, 0) : 0;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(walkFraction + landingFraction, 5) * 0.02f, ref _viewBobVelocity, 0.075f);
            }

            // camera bob
            if (ModMain.IsViewBobEnabled)
            {
                Vector2 cameraBob = new Vector2(Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity, Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity);
                _cameraRoot.transform.Translate(new Vector3(cameraBob.x * ModMain.ViewBobXAmount, cameraBob.y * ModMain.ViewBobYAmount));
                RotateCamera(new Vector3(-cameraBob.y * 5 * ModMain.ViewBobPitchAmount, 0, -cameraBob.x * 5 * ModMain.ViewBobRollAmount));
            }

            // tool bob
            if (ModMain.IsToolBobEnabled)
            {
                toolBob = new Vector3(Mathf.Sin(_viewBobTime * 6.28318f) * _viewBobIntensity * 0.25f, Mathf.Cos(_viewBobTime * 12.5664f) * _viewBobIntensity * 0.25f);
                toolBob.z = -toolBob.x * (ModMain.IsLeftyModeEnabled ? -1 : 1);
                _mainToolRoot.transform.localPosition = new Vector3(0, toolBob.y * ModMain.ToolBobYAmount);
                _mainToolRoot.transform.localRotation = Quaternion.Euler(new Vector3(toolBob.y * 100 * ModMain.ToolBobPitchAmount, 0, -toolBob.x * 100 * ModMain.ToolBobRollAmount));
                _mainToolRoot.transform.Translate(new Vector3(toolBob.x * ModMain.ToolBobXAmount, 0, toolBob.z * ModMain.ToolBobZAmount), _characterController.transform);
            }
        }

        if (ModMain.IsToolSwayEnabled)
        {
            UpdateToolSway();
        }
        else
        {
            _toolSway = Vector3.zero;
            _toolSwayVelocity = Vector3.zero;
        }

        if (ModMain.DynamicToolPosBehavior != "Disabled")
        {
            _mainToolRoot.transform.localPosition += GetDynamicToolPos();
        }

        // Probe Launcher position offset needs to be 3x bigger because the tools in it are further away and appear to move less
        _probeLauncherRoot.transform.localPosition = 3 * _mainToolRoot.transform.localPosition;
        _probeLauncherRoot.transform.localRotation = _mainToolRoot.transform.localRotation;
        if (ModMain.IsScoutAnimEnabled)
        {
            ApplyScoutAnim();
        }

        // Translator offset needs to be 3x bigger, also needs to convert forward bob into additional sideways bob
        _translatorRoot.transform.localPosition = 3 * _mainToolRoot.transform.localPosition;
        _translatorRoot.transform.localRotation = _mainToolRoot.transform.localRotation;
        _translatorRoot.transform.Translate(3 * new Vector3(toolBob.x * (new Vector2(ModMain.ToolBobXAmount, ModMain.ToolBobZAmount).magnitude - ModMain.ToolBobXAmount), 0, ModMain.ToolBobZAmount * -toolBob.z), _characterController.transform);

        if (ModMain.IsHideStowedItemsEnabled)
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
        _cameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.right), eulers.x);
        _cameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.up), eulers.y);
        _cameraRoot.transform.RotateAround(_cameraController.transform.position, _cameraController.transform.TransformDirection(Vector3.forward), eulers.z);
    }

    private void UpdateToolSway()
    {
        Vector2 lookDelta = Vector2.zero;
        if (OWInput.IsInputMode(InputMode.Character) && !(PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
        {
            // look input code lifted directly from the game. no touch!
            lookDelta = OWInput.GetAxisValue(InputLibrary.look) * _characterController._playerCam.fieldOfView / _characterController._initFOV * 0.002f * Time.deltaTime / (Time.timeScale != 0 ? Time.timeScale : 1);
            bool isAlarming = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            lookDelta *= _characterController._signalscopeZoom || isAlarming ? PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR : PlayerCameraController.LOOK_RATE;
        }

        lookDelta *= 5;
        float degreesY = _cameraController.GetDegreesY();
        // cancel out vertical sway if the player can't turn anymore in that direction
        if ((lookDelta.y > 0 && degreesY >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0 && degreesY <= PlayerCameraController._minDegreesYNormal))
        {
            lookDelta.y = 0;
        }

        // decay already existing tool sway and then add new tool sway
        _toolSway = Vector2.SmoothDamp(_toolSway, Vector2.zero, ref _toolSwayVelocity, 0.2f * ModMain.ToolSwaySmoothing, 5);
        _toolSway = Vector2.ClampMagnitude(_toolSway - lookDelta * (1 - _toolSway.magnitude), 1);
        float localZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.y) - 1);
        float globalZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.x) - 1);
        float xSwayMultiplier = (Mathf.Cos(degreesY * 0.03490f) + 1) * 0.5f;

        _mainToolRoot.transform.localPosition += 0.15f * ModMain.ToolSwayTranslateAmount * new Vector3(0, _toolSway.y, localZOffset);
        _mainToolRoot.transform.localRotation *= Quaternion.Euler(-20 * ModMain.ToolSwayRotateAmount * new Vector3(_toolSway.y, 0, 0));
        _mainToolRoot.transform.Translate(0.15f * xSwayMultiplier * ModMain.ToolSwayTranslateAmount * new Vector3(_toolSway.x, 0, globalZOffset), _characterController.transform);
        _mainToolRoot.transform.RotateAround(_characterController.transform.position, _characterController._owRigidbody.GetLocalUpDirection(), 20 * ModMain.ToolSwayRotateAmount * _toolSway.x);
    }

    private Vector3 GetDynamicToolPos()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolPos;
        if (ModMain.DynamicToolPosBehavior == "Legacy")
        {
            // new behavior moves tool closer to camera the more you are looking up/down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * ModMain.DynamicToolPosYAmount, -degreesY * 0.01111f * ModMain.DynamicToolPosZAmount) * 0.04f;
        }
        else
        {
            // legacy behavior moves tool closer when looking up and further when looking down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * ModMain.DynamicToolPosYAmount, (Mathf.Cos(degreesY * 0.03490f) - 1) * 0.3f * ModMain.DynamicToolPosZAmount) * 0.04f;
        }

        return dynamicToolPos;
    }

    // plays a recoil animation for 0.5 seconds after scout launch
    private void ApplyScoutAnim()
    {
        float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0) * 2;
        float dampTime = targetRecoil > _scoutRecoil ? 0.05f : 0.1f;
        _scoutRecoil = Mathf.SmoothDamp(_scoutRecoil, targetRecoil, ref _scoutRecoilVelocity, dampTime);

        RotateCamera(new Vector3(-5, 0, ModMain.IsLeftyModeEnabled ? 5 : -5) * _scoutRecoil);
        _probeLauncherRoot.transform.localPosition += new Vector3(ModMain.IsLeftyModeEnabled ? -0.25f : 0.25f, -0.25f, -0.5f) * _scoutRecoil;
        _probeLauncherRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-15, 0, ModMain.IsLeftyModeEnabled ? 15 : -15) * _scoutRecoil);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void AddToPlayerCamera(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<CameraViewmodelManager>();
    }
}
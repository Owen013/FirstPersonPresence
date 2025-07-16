using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewbobManager : MonoBehaviour
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
            if (ModMain.Instance.IsScoutAnimEnabled)
            {
                _lastScoutLaunchTime = Time.time;
            }
        };
        ModMain.Instance.OnConfigure += () =>
        {
            if (ModMain.Instance.IsLeftyModeEnabled)
            {
                _mainToolRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
                _probeLauncherRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                _mainToolRoot.transform.localScale = Vector3.one;
                _probeLauncherRoot.transform.localScale = Vector3.one;
            }
        };

        if (ModMain.Instance.IsLeftyModeEnabled)
        {
            _mainToolRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
            _probeLauncherRoot.transform.localScale = new Vector3(-1f, 1f, 1f);
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
        if (ModMain.Instance.IsViewBobEnabled || ModMain.Instance.IsToolBobEnabled)
        {
            //float predictedViewBobTime = _viewBobTime + 1.033333f * _animController._animator.speed * Time.deltaTime;
            //float animatorTime = _animController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.25f;
            //_viewBobTime = Mathf.Floor(animatorTime) + Mathf.Repeat(Mathf.Clamp(animatorTime, predictedViewBobTime - 0.3f * Time.deltaTime, predictedViewBobTime + 0.3f * Time.deltaTime), 1f);
            _viewBobTime += 1.033333f * _animController._animator.speed * Time.deltaTime;

            if (!_characterController.IsGrounded() && !_characterController._isMovementLocked)
            {
                // if in midair, use falling and/or jumping animation
                float fallFraction = ModMain.Instance.IsFallAnimEnabled ? _animController._animator.GetFloat("FreefallSpeed") : 0f;
                float jumpFraction = ModMain.Instance.IsJumpAnimEnabled ? Mathf.Max((_characterController._lastJumpTime + 0.5f - Time.time) * 2f, 0f) : 0f;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(fallFraction + jumpFraction, 2f), ref _viewBobVelocity, 0.075f);
            }
            else
            {
                // if on ground, use walking and/or landing animation
                float walkFraction = 0.5f * new Vector2(_animController._animator.GetFloat("RunSpeedX"), _animController._animator.GetFloat("RunSpeedY")).magnitude;
                float landingFraction = ModMain.Instance.IsLandingAnimEnabled && Time.timeSinceLevelLoad > 1f ? 0.5f * Mathf.Max((_lastLandedTime + 0.25f - Time.time) * 6f, 0f) : 0f;
                _viewBobIntensity = Mathf.SmoothDamp(_viewBobIntensity, Mathf.Min(walkFraction + landingFraction, 2f), ref _viewBobVelocity, 0.075f);
            }

            // camera bob
            if (ModMain.Instance.IsViewBobEnabled)
            {
                Vector2 cameraBob = 0.04f * _viewBobIntensity * new Vector2(Mathf.Sin(_viewBobTime * 6.28318f), Mathf.Cos(_viewBobTime * 12.5664f));
                _cameraRoot.transform.Translate((ModMain.Instance.SmolHatchlingAPI?.GetPlayerScale() ?? 1f) * new Vector3(cameraBob.x * ModMain.Instance.ViewBobXAmount, cameraBob.y * ModMain.Instance.ViewBobYAmount));
                RotateCamera(5f * new Vector3(cameraBob.y * ModMain.Instance.ViewBobPitchAmount, 0f, -cameraBob.x * ModMain.Instance.ViewBobRollAmount));
            }

            // tool bob
            if (ModMain.Instance.IsToolBobEnabled)
            {
                toolBob = 0.01f * _viewBobIntensity * new Vector3(Mathf.Sin(_viewBobTime * 6.28318f), Mathf.Cos(_viewBobTime * 12.5664f));
                toolBob.z = toolBob.x * (ModMain.Instance.IsLeftyModeEnabled ? 1f : -1f);
                _mainToolRoot.transform.localPosition = new Vector3(0f, toolBob.y * ModMain.Instance.ToolBobYAmount);
                _mainToolRoot.transform.localRotation = Quaternion.Euler(100f * new Vector3(toolBob.y * ModMain.Instance.ToolBobPitchAmount, 0f, -toolBob.x * ModMain.Instance.ToolBobRollAmount));
                _mainToolRoot.transform.Translate((ModMain.Instance.SmolHatchlingAPI?.GetPlayerScale() ?? 1f) * new Vector3(toolBob.x * ModMain.Instance.ToolBobXAmount, 0f, toolBob.z * ModMain.Instance.ToolBobZAmount), _characterController.transform);
            }
        }

        if (ModMain.Instance.IsToolSwayEnabled)
        {
            UpdateToolSway();
        }
        else
        {
            _toolSway = Vector3.zero;
            _toolSwayVelocity = Vector3.zero;
        }

        if (ModMain.Instance.DynamicToolPosBehavior != "Disabled")
        {
            _mainToolRoot.transform.localPosition += GetDynamicToolPos();
        }

        // Probe Launcher position offset needs to be 3x bigger because the tools in it are further away and appear to move less
        _probeLauncherRoot.transform.localPosition = 3f * _mainToolRoot.transform.localPosition;
        _probeLauncherRoot.transform.localRotation = _mainToolRoot.transform.localRotation;
        if (ModMain.Instance.IsScoutAnimEnabled)
        {
            ApplyScoutAnim();
        }

        // Translator offset needs to be 3x bigger, also needs to convert forward bob into additional sideways bob
        _translatorRoot.transform.localPosition = 3f * _mainToolRoot.transform.localPosition;
        _translatorRoot.transform.localRotation = _mainToolRoot.transform.localRotation;
        _translatorRoot.transform.Translate(3f * (ModMain.Instance.SmolHatchlingAPI?.GetPlayerScale() ?? 1f) * new Vector3(toolBob.x * (new Vector2(ModMain.Instance.ToolBobXAmount, ModMain.Instance.ToolBobZAmount).magnitude - ModMain.Instance.ToolBobXAmount), 0f, ModMain.Instance.ToolBobZAmount * -toolBob.z), _characterController.transform);

        if (ModMain.Instance.IsHideStowedItemsEnabled)
        {
            ItemTool itemTool = Locator.GetToolModeSwapper()._itemCarryTool;
            if (!itemTool.IsEquipped() && !itemTool.IsPuttingAway())
            {
                itemTool.transform.localRotation = Quaternion.RotateTowards(itemTool.transform.localRotation, Quaternion.Euler(180f, 0f, 0f), 180f * Time.deltaTime);
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
        float degreesY = _cameraController.GetDegreesY();

        // get look delta
        if (!(PlayerState.InZeroG() && PlayerState.IsWearingSuit()) && OWInput.IsInputMode(InputMode.Character | InputMode.PatchingSuit))
        {
            lookDelta = 0.01f * OWInput.GetAxisValue(InputLibrary.look) * (_characterController._playerCam.fieldOfView / _characterController._initFOV);
            lookDelta *= InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : Time.deltaTime;

            AlarmSequenceController alarm = Locator.GetAlarmSequenceController();
            bool isAlarmWakingPlayer = alarm != null && alarm.IsAlarmWakingPlayer();
            lookDelta *= isAlarmWakingPlayer ? PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR : PlayerCameraController.LOOK_RATE;

            if (Time.timeScale > 1f)
            {
                lookDelta /= Time.timeScale;
            }

            // cancel out horizontal sway if player is patching suit, as they can't turn left/right while doing so
            if (OWInput.IsInputMode(InputMode.PatchingSuit))
            {
                lookDelta.x = 0f;
            }

            // cancel out vertical sway if the player can't turn anymore in that direction
            if ((lookDelta.y > 0f && degreesY >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0f && degreesY <= PlayerCameraController._minDegreesYNormal))
            {
                lookDelta.y = 0f;
            }
        }

        // decay already existing tool sway and then add new tool sway
        _toolSway = Vector2.SmoothDamp(_toolSway, Vector2.zero, ref _toolSwayVelocity, 0.2f * ModMain.Instance.ToolSwaySmoothing, 5f);
        _toolSway = Vector2.ClampMagnitude(_toolSway - lookDelta * (1f - _toolSway.magnitude), 1f);
        float localZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.y) - 1f);
        float globalZOffset = 0.15f * (Mathf.Cos(Mathf.PI * _toolSway.x) - 1f);
        float xSwayMultiplier = (Mathf.Cos(degreesY * 0.03490f) + 1f) * 0.5f;

        _mainToolRoot.transform.localPosition += 0.15f * ModMain.Instance.ToolSwayTranslateAmount * new Vector3(0, _toolSway.y, localZOffset);
        _mainToolRoot.transform.localRotation *= Quaternion.Euler(-15f * ModMain.Instance.ToolSwayRotateAmount * new Vector3(_toolSway.y, 0f, 0f));
        _mainToolRoot.transform.Translate(0.15f * xSwayMultiplier * ModMain.Instance.ToolSwayTranslateAmount * (ModMain.Instance.SmolHatchlingAPI?.GetPlayerScale() ?? 1f) * new Vector3(_toolSway.x, 0f, globalZOffset), _characterController.transform);
        _mainToolRoot.transform.RotateAround(_characterController.transform.position, _characterController._owRigidbody.GetLocalUpDirection(), 15f * _toolSway.x * ModMain.Instance.ToolSwayRotateAmount);
    }

    private Vector3 GetDynamicToolPos()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolPos;
        if (ModMain.Instance.DynamicToolPosBehavior == "Legacy")
        {
            // legacy behavior moves tool closer when looking up and further when looking down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * ModMain.Instance.DynamicToolPosYAmount, -degreesY * 0.01111f * ModMain.Instance.DynamicToolPosZAmount) * 0.04f;
        }
        else
        {
            // new behavior moves tool closer to camera the more you are looking up/down
            dynamicToolPos = new Vector3(0f, -degreesY * 0.02222f * ModMain.Instance.DynamicToolPosYAmount, (Mathf.Cos(degreesY * 0.03490f) - 1f) * 0.3f * ModMain.Instance.DynamicToolPosZAmount) * 0.04f;
        }

        return dynamicToolPos;
    }

    // plays a recoil animation for 0.5 seconds after scout launch
    private void ApplyScoutAnim()
    {
        float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
        float dampTime = targetRecoil > _scoutRecoil ? 0.05f : 0.1f;
        _scoutRecoil = Mathf.SmoothDamp(_scoutRecoil, targetRecoil, ref _scoutRecoilVelocity, dampTime);

        RotateCamera(new Vector3(-5f, 0f, ModMain.Instance.IsLeftyModeEnabled ? 5f : -5f) * _scoutRecoil);
        _probeLauncherRoot.transform.localPosition += new Vector3(ModMain.Instance.IsLeftyModeEnabled ? -0.25f : 0.25f, -0.25f, -0.5f) * _scoutRecoil;
        _probeLauncherRoot.transform.localRotation *= Quaternion.Euler(new Vector3(-15f, 0f, ModMain.Instance.IsLeftyModeEnabled ? 15f : -15f) * _scoutRecoil);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Start))]
    private static void AddToPlayerCamera(PlayerCameraController __instance)
    {
        __instance.gameObject.AddComponent<ViewbobManager>();
    }
}
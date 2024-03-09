using UnityEngine;

namespace Immersion.Components;

public class CameraMovementController : MonoBehaviour
{
    public static CameraMovementController Instance { get; private set; }
    public GameObject CameraRoot { get; private set; }
    public GameObject ToolRoot { get; private set; }
    public GameObject BigToolRoot { get; private set; }

    private readonly float _bigRootTransformMultiplier = 3f;
    private readonly float _maxSwayMagnitude = 0.25f;
    private readonly float _bobIntensityStepSize = 0.25f;

    private PlayerCameraController _cameraController;
    private PlayerAnimController _animController;
    private float _viewBobTimePosition;
    private float _viewBobIntensity;
    private Vector3 _currentToolSway;
    private Vector3 _toolSwayVelocity;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cameraController = GetComponent<PlayerCameraController>();
        _animController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>();

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
    }

    private void Update()
    {
        UpdateViewBob();

        if (Config.ToolHeightYAmount != 0f || Config.ToolHeightZAmount != 0f)
        {
            ApplyDynamicToolHeight();
        }
        if (Config.ToolSwaySensitivity != 0f || _currentToolSway != Vector3.zero)
        {
            ApplyToolSway();
        }

        BigToolRoot.transform.localPosition = ToolRoot.transform.localPosition * _bigRootTransformMultiplier;
    }

    private void UpdateViewBob()
    {
        _viewBobTimePosition = Mathf.Repeat(_viewBobTimePosition + Time.deltaTime * 1.033333f * _animController._animator.speed, 1f);
        _viewBobIntensity = Mathf.MoveTowards(_viewBobIntensity, Mathf.Sqrt(Mathf.Pow(_animController._animator.GetFloat("RunSpeedX"), 2f) + Mathf.Pow(_animController._animator.GetFloat("RunSpeedY"), 2f)) * 0.02f, _bobIntensityStepSize * Time.deltaTime);
        
        // camera bob
        float bobX = Mathf.Sin(_viewBobTimePosition * 6.28318f) * _viewBobIntensity;
        float bobY = Mathf.Cos(_viewBobTimePosition * 12.5664f) * _viewBobIntensity;
        if (Main.Instance.SmolHatchlingAPI != null)
        {
            bobX *= Main.Instance.SmolHatchlingAPI.GetCurrentScale().x;
            bobY *= Main.Instance.SmolHatchlingAPI.GetCurrentScale().y;
        }
        CameraRoot.transform.localPosition = new Vector3(bobX * Config.ViewBobXAmount, bobY * Config.ViewBobYAmount, 0f);
        CameraRoot.transform.localRotation = Quaternion.Euler(new Vector3(bobY * 5f * Config.ViewBobPitchAmount, 0f, bobX * 5f * Config.ViewBobRollAmount));

        // tool bob
        float toolBobX = Mathf.Sin(_viewBobTimePosition * 6.28318f) * _viewBobIntensity * Config.ToolBobXAmount * 0.25f;
        float toolBobY = Mathf.Cos(_viewBobTimePosition * 12.5664f) * _viewBobIntensity * Config.ToolBobYAmount * 0.25f;
        float toolBobZ = -Mathf.Sin(_viewBobTimePosition * 6.28318f) * _viewBobIntensity * Config.ToolBobZAmount * 0.25f;
        ToolRoot.transform.localPosition = new Vector3(toolBobX, toolBobY, toolBobZ);
        ToolRoot.transform.localRotation = Quaternion.Euler(new Vector3(bobY * 25f * Config.ToolBobPitchAmount, 0f, bobX * 25f * Config.ToolBobRollAmount));
        BigToolRoot.transform.localRotation = ToolRoot.transform.localRotation;
    }

    private void ApplyDynamicToolHeight()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolHeight = new Vector3(0f, -degreesY * 0.02222f * Config.ToolHeightYAmount, (Mathf.Cos(degreesY * 0.03490f) - 1) * 0.3f * Config.ToolHeightZAmount) * 0.04f;
        ToolRoot.transform.localPosition += dynamicToolHeight;
    }


    private void ApplyToolSway()
    {
        // get input
        Vector2 lookDelta;
        if (!OWInput.IsInputMode(InputMode.Character) || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
        {
            lookDelta = Vector2.zero;
        }
        else
        {
            lookDelta = OWInput.GetAxisValue(InputLibrary.look);
        }
        lookDelta *= 0.25f * Time.deltaTime * Config.ToolSwaySensitivity;

        float degreesY = _cameraController.GetDegreesY();
        lookDelta.x *= (Mathf.Cos(degreesY * 0.03490f) + 1f) * 0.5f;
        if ((lookDelta.y > 0f && degreesY >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0f && degreesY <= PlayerCameraController._minDegreesYNormal))
        {
            lookDelta.y = 0f;
        }

        _currentToolSway = Vector3.SmoothDamp(_currentToolSway, Vector3.zero, ref _toolSwayVelocity, 0.2f * Config.ToolSwaySmoothing);
        _currentToolSway += new Vector3(-lookDelta.x, -lookDelta.y, 0f) * (_maxSwayMagnitude - _currentToolSway.magnitude) / _maxSwayMagnitude;
        _currentToolSway.z = Mathf.Cos(_currentToolSway.magnitude * 1.57080f) - 1f;

        ToolRoot.transform.localPosition += _currentToolSway;
    }
}
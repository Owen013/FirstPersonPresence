using UnityEngine;

namespace FirstPersonPresence.Components;

public class ViewBobController : MonoBehaviour
{
    public static ViewBobController Instance;
    public GameObject viewBobRoot;
    public GameObject toolRoot;
    public GameObject probeLauncherRoot;
    private PlayerCameraController _cameraController;
    private PlayerAnimController _animController;
    private float _viewBobTimePosition;
    private float _viewBobIntensity;
    private const float _probeLauncherRootTransformMultiplier = 3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cameraController = Locator.GetPlayerCameraController();
        _animController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>();

        // create view bob root and parent camera to it
        viewBobRoot = new();
        viewBobRoot.name = "ViewBobRoot";
        viewBobRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform.parent;
        viewBobRoot.transform.localPosition = Vector3.zero;
        viewBobRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.parent = viewBobRoot.transform;

        // create tool root and parent tools to it
        toolRoot = new();
        toolRoot.name = "ToolRoot";
        toolRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        toolRoot.transform.localPosition = Vector3.zero;
        toolRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ItemCarryTool").transform.parent = toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("FlashlightRoot").transform.parent = toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("Signalscope").transform.parent = toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("NomaiTranslatorProp").transform.parent = toolRoot.transform;

        // create a separate root for the scout launcher since it's seemingly less reactive to transformations
        probeLauncherRoot = new();
        probeLauncherRoot.name = "ProbeLauncherRoot";
        probeLauncherRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        probeLauncherRoot.transform.localPosition = Vector3.zero;
        probeLauncherRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ProbeLauncher").transform.parent = probeLauncherRoot.transform;
    }

    private void Update()
    {
        UpdateViewBob();
        ApplyDynamicToolHeight();
    }

    private void UpdateViewBob()
    {
        _viewBobTimePosition = Mathf.Repeat(_viewBobTimePosition + Time.deltaTime * 1.03f * _animController._animator.speed, 1);
        _viewBobIntensity = Mathf.Lerp(_viewBobIntensity, Mathf.Sqrt(Mathf.Pow(_animController._animator.GetFloat("RunSpeedX"), 2f) + Mathf.Pow(_animController._animator.GetFloat("RunSpeedY"), 2f)) * 0.02f, 0.25f);
        
        // camera bob
        float bobX = Mathf.Sin(2f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.viewBobXSensitivity;
        float bobY = Mathf.Cos(4f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.viewBobYSensitivity;
        if (Main.Instance.SmolHatchlingAPI != null)
        {
            bobX *= Main.Instance.SmolHatchlingAPI != null ? Main.Instance.SmolHatchlingAPI.GetCurrentScale().x : 1f;
            bobY *= Main.Instance.SmolHatchlingAPI != null ? Main.Instance.SmolHatchlingAPI.GetCurrentScale().y : 1f;
        }
        viewBobRoot.transform.localPosition = new Vector3(bobX, bobY, 0f);

        // tool bob
        float toolBobX = Mathf.Sin(2f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.toolBobSensitivity * 0.5f;
        float toolBobY = Mathf.Cos(4f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.toolBobSensitivity * 0.25f;
        toolRoot.transform.localPosition = new Vector3(toolBobX, toolBobY, 0f);
        probeLauncherRoot.transform.localPosition = toolRoot.transform.localPosition * _probeLauncherRootTransformMultiplier;
    }

    private void ApplyDynamicToolHeight()
    {
        Vector3 dynamicToolHeight = new Vector3(0f, -_cameraController.GetDegreesY() * 0.02222f * Main.Instance.toolHeightYSensitivity, -(Mathf.Cos(Mathf.PI * _cameraController.GetDegreesY() * 0.01111f) - 1) * 0.3f * Main.Instance.toolHeightZSensitivity) * 0.03f;
        toolRoot.transform.localPosition += dynamicToolHeight;
        probeLauncherRoot.transform.localPosition += dynamicToolHeight * _probeLauncherRootTransformMultiplier;
    }
}

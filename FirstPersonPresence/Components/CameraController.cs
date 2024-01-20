﻿using HarmonyLib;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace FirstPersonPresence.Components;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private PlayerCameraController _cameraController;
    private PlayerAnimController _animController;
    private GameObject _cameraRoot;
    private GameObject _toolRoot;
    private GameObject _probeLauncherRoot;
    private float _viewBobTimePosition;
    private float _viewBobIntensity;
    private Vector3 _currentToolSway;
    private Vector3 _toolSwayVelocity;
    private const float MAX_SWAY_MAGNITUDE = 0.25f;
    private float ZERO_G_SWAY_MULTIPLIER = 0.25f;
    private const float PROBE_LAUNCHER_TRANSFORM_MULTIPLIER = 3f;

    public GameObject GetCameraRoot() => _cameraRoot;

    public GameObject GetToolRoot() => _toolRoot;

    public GameObject GetProbeLauncherRoot() => _probeLauncherRoot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _cameraController = GetComponent<PlayerCameraController>();
        _animController = Locator.GetPlayerBody().GetComponentInChildren<PlayerAnimController>();

        // create view bob root and parent camera to it
        _cameraRoot = new();
        _cameraRoot.name = "CameraRoot";
        _cameraRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform.parent;
        _cameraRoot.transform.localPosition = Vector3.zero;
        _cameraRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.parent = _cameraRoot.transform;

        // create tool root and parent tools to it
        _toolRoot = new();
        _toolRoot.name = "ToolRoot";
        _toolRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        _toolRoot.transform.localPosition = Vector3.zero;
        _toolRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ItemCarryTool").transform.parent = _toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("FlashlightRoot").transform.parent = _toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("Signalscope").transform.parent = _toolRoot.transform;
        _cameraController._playerCamera.mainCamera.transform.Find("NomaiTranslatorProp").transform.parent = _toolRoot.transform;

        // create a separate root for the scout launcher since it's seemingly less reactive to transformations
        _probeLauncherRoot = new();
        _probeLauncherRoot.name = "ProbeLauncherRoot";
        _probeLauncherRoot.transform.parent = _cameraController._playerCamera.mainCamera.transform;
        _probeLauncherRoot.transform.localPosition = Vector3.zero;
        _probeLauncherRoot.transform.localRotation = Quaternion.identity;
        _cameraController._playerCamera.mainCamera.transform.Find("ProbeLauncher").transform.parent = _probeLauncherRoot.transform;
    }

    private void Update()
    {
        UpdateViewBob();
        ApplyDynamicToolHeight();
        ApplyToolSway();

        _probeLauncherRoot.transform.localPosition = _toolRoot.transform.localPosition * PROBE_LAUNCHER_TRANSFORM_MULTIPLIER;
    }

    private void UpdateViewBob()
    {
        _viewBobTimePosition = Mathf.Repeat(_viewBobTimePosition + Time.deltaTime * 1.03f * _animController._animator.speed, 1);
        _viewBobIntensity = Mathf.Lerp(_viewBobIntensity, Mathf.Sqrt(Mathf.Pow(_animController._animator.GetFloat("RunSpeedX"), 2f) + Mathf.Pow(_animController._animator.GetFloat("RunSpeedY"), 2f)) * 0.02f, 0.25f);
        
        // camera bob
        float bobX = Mathf.Sin(2f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.ViewBobXAmount;
        float bobY = Mathf.Cos(4f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.ViewBobYAmount;
        if (Main.Instance.SmolHatchlingAPI != null)
        {
            bobX *= Main.Instance.SmolHatchlingAPI != null ? Main.Instance.SmolHatchlingAPI.GetCurrentScale().x : 1f;
            bobY *= Main.Instance.SmolHatchlingAPI != null ? Main.Instance.SmolHatchlingAPI.GetCurrentScale().y : 1f;
        }
        _cameraRoot.transform.localPosition = new Vector3(bobX, bobY, 0f);

        // tool bob
        float toolBobX = Mathf.Sin(2f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.ToolBobAmount * 0.5f;
        float toolBobY = Mathf.Cos(4f * Mathf.PI * _viewBobTimePosition) * _viewBobIntensity * Main.Instance.ToolBobAmount * 0.25f;
        _toolRoot.transform.localPosition = new Vector3(toolBobX, toolBobY, 0f);
    }

    private void ApplyDynamicToolHeight()
    {
        float degreesY = _cameraController.GetDegreesY();
        Vector3 dynamicToolHeight = new Vector3(0f, -degreesY * 0.02222f * Main.Instance.ToolHeightYAmount, (Mathf.Cos(Mathf.PI * degreesY * 0.01111f) - 1) * 0.3f * Main.Instance.ToolHeightZAmount) * 0.04f;
        _toolRoot.transform.localPosition += dynamicToolHeight;
    }


    private void ApplyToolSway()
    {
        // get input
        Vector2 lookDelta;
        if (!OWInput.IsInputMode(InputMode.Character))
        {
            lookDelta = Vector2.zero;
        }
        else if (PlayerState.InZeroG() && PlayerState.IsWearingSuit())
        {
            lookDelta = new Vector2(OWInput.GetValue(InputLibrary.yaw), OWInput.GetValue(InputLibrary.pitch)) * ZERO_G_SWAY_MULTIPLIER;
        }
        else
        {
            lookDelta = OWInput.GetAxisValue(InputLibrary.look);
        }
        lookDelta *= 0.25f * Time.deltaTime * Main.Instance.ToolSwaySensitivity;

        if ((lookDelta.y > 0 && _cameraController.GetDegreesY() >= PlayerCameraController._maxDegreesYNormal) || (lookDelta.y < 0 && _cameraController.GetDegreesY() <= PlayerCameraController._minDegreesYNormal))
        {
            lookDelta.y = 0f;
        }

        _currentToolSway = Vector3.SmoothDamp(_currentToolSway, Vector3.zero, ref _toolSwayVelocity, 0.25f * Main.Instance.ToolSwaySmoothing);
        _currentToolSway += new Vector3(-lookDelta.x, -lookDelta.y, 0f) * (MAX_SWAY_MAGNITUDE - _currentToolSway.magnitude) / MAX_SWAY_MAGNITUDE;
        _currentToolSway.z = Mathf.Cos(Mathf.PI * _currentToolSway.magnitude * 0.5f) - 1f;

        _toolRoot.transform.localPosition += _currentToolSway;
    }
}
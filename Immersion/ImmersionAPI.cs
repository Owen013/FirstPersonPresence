using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public GameObject GetCameraRoot() => CameraMovementController.Instance.CameraRoot;

    public GameObject GetToolRoot() => CameraMovementController.Instance.ToolRoot;

    public GameObject GetProbeLauncherRoot() => CameraMovementController.Instance.ProbeLauncherRoot;

    public GameObject GetTranslatorRoot() => CameraMovementController.Instance.TranslatorRoot;
}
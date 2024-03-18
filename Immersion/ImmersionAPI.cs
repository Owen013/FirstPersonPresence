using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public GameObject GetCameraRoot() => ImmersionController.Instance.CameraRoot;

    public GameObject GetToolRoot() => ImmersionController.Instance.ToolRoot;

    public GameObject GetProbeLauncherRoot() => ImmersionController.Instance.ProbeLauncherRoot;

    public GameObject GetTranslatorRoot() => ImmersionController.Instance.TranslatorRoot;
}
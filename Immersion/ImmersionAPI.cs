using Immersion.Components;
using UnityEngine;

namespace Immersion;

public class ImmersionAPI
{
    public GameObject GetCameraRoot() => CameraMovementController.Instance.CameraRoot;

    public GameObject GetToolRoot() => CameraMovementController.Instance.ToolRoot;

    public GameObject GetBigToolRoot() => CameraMovementController.Instance.BigToolRoot;
}
using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetCameraRoot() => CameraMovementController.Instance.CameraRoot;

    public GameObject GetToolRoot() => CameraMovementController.Instance.ToolRoot;

    public GameObject GetBigToolRoot() => CameraMovementController.Instance.BigToolRoot;
}
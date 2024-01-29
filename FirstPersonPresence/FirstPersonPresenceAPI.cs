using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetCameraRoot() => RootController.Instance.CameraRoot;

    public GameObject GetToolRoot() => RootController.Instance.ToolRoot;

    public GameObject GetBigToolRoot() => RootController.Instance.BigToolRoot;
}
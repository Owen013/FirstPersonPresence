using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetCameraRoot() => CameraController.Instance.GetProbeLauncherRoot();

    public GameObject GetToolRoot() => CameraController.Instance.GetProbeLauncherRoot();

    public GameObject GetProbeLauncherRoot() => CameraController.Instance.GetProbeLauncherRoot();
}
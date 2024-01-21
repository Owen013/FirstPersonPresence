using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetCameraRoot() => RootController.Instance.GetBigToolRoot();

    public GameObject GetToolRoot() => RootController.Instance.GetBigToolRoot();

    public GameObject GetProbeLauncherRoot() => RootController.Instance.GetBigToolRoot();
}
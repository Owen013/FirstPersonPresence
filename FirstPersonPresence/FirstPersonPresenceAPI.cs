using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetViewBobRoot()
    {
        return CameraController.Instance.viewBobRoot;
    }

    public GameObject GetToolRoot()
    {
        return CameraController.Instance.toolRoot;
    }

    public GameObject GetProbeLauncherRoot()
    {
        return CameraController.Instance.probeLauncherRoot;
    }
}
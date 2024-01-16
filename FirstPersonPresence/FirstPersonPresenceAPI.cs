using FirstPersonPresence.Components;
using UnityEngine;

namespace FirstPersonPresence;

public class FirstPersonPresenceAPI
{
    public GameObject GetViewBobRoot()
    {
        return ViewBobController.Instance.viewBobRoot;
    }

    public GameObject GetToolRoot()
    {
        return ViewBobController.Instance.toolRoot;
    }

    public GameObject GetProbeLauncherRoot()
    {
        return ViewBobController.Instance.probeLauncherRoot;
    }
}
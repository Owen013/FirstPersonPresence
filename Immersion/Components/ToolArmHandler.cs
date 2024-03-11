using System.CodeDom;
using UnityEngine;

namespace Immersion.Components;

public class ToolArmHandler : MonoBehaviour
{
    private void Start()
    {
        NewToolArm(GameObject.Find("Player_Body/ShakeRoot/CameraRoot/PlayerCamera/ToolRoot/Signalscope/Props_HEA_Signalscope").transform, new Vector3(0.01f, -0.09f, -0.21f), new Vector3(0.4f, 0.4f, 0.4f)).AddComponent<SignalscopeArm>();
    }

    private GameObject NewToolArm(Transform parent, Vector3 localPos, Vector3 scale)
    {
        GameObject arm = new("Arm");
        arm.transform.parent = parent;
        arm.transform.localPosition = localPos;
        arm.transform.localRotation = Quaternion.identity;
        arm.transform.localScale = scale;

        GameObject noSuit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        noSuit.name = "NoSuit";
        noSuit.transform.parent = arm.transform;
        noSuit.transform.localPosition = Vector3.zero;
        noSuit.transform.localRotation = Quaternion.identity;
        noSuit.transform.localScale = Vector3.one;
        foreach (Material material in noSuit.GetComponent<MeshRenderer>().materials)
        {
            material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
            material.shader = parent.GetComponent<MeshRenderer>().material.shader;
        }

        GameObject suit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        suit.name = "Suit";
        suit.transform.parent = arm.transform;
        suit.transform.localPosition = Vector3.zero;
        suit.transform.localRotation = Quaternion.identity;
        suit.transform.localScale = Vector3.one;
        foreach (Material material in suit.GetComponent<MeshRenderer>().materials)
        {
            material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
            material.shader = parent.GetComponent<MeshRenderer>().material.shader;
        }

        return arm;
    }
}
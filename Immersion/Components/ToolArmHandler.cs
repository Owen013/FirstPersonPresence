using System.CodeDom;
using UnityEngine;

namespace Immersion.Components;

public class ToolArmHandler : MonoBehaviour
{
    public static GameObject NewArm(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false)
    {
        if (parent == null)
        {
            Main.Instance.Log($"Can't create new arm; parent is null");
            return null;
        }

        GameObject arm = new("Arm");
        arm.transform.parent = parent;
        arm.transform.localPosition = localPos;
        arm.transform.localRotation = localRot;
        arm.transform.localScale = scale;

        GameObject noSuit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        noSuit.name = "NoSuit";
        noSuit.transform.parent = arm.transform;
        noSuit.transform.localPosition = Vector3.zero;
        noSuit.transform.localRotation = Quaternion.Euler(330f, 0f, 300f);
        noSuit.transform.localScale = Vector3.one;

        GameObject suit = Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        suit.name = "Suit";
        suit.transform.parent = arm.transform;
        suit.transform.localPosition = new Vector3(-0.02f, 0.03f, 0.02f);
        suit.transform.localRotation = Quaternion.Euler(330f, 0f, 300f);
        suit.transform.localScale = Vector3.one;

        if (!useDefaultShader)
        {
            foreach (Material material in noSuit.GetComponent<MeshRenderer>().materials)
            {
                material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
                material.shader = parent.GetComponent<MeshRenderer>().material.shader;
            }
            foreach (Material material in suit.GetComponent<MeshRenderer>().materials)
            {
                material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
                material.shader = parent.GetComponent<MeshRenderer>().material.shader;
            }
        }

        return arm;
    }
}
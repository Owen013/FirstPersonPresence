using UnityEngine;

namespace Immersion.Components;

public class ToolArmHandler : MonoBehaviour
{
    public static GameObject NewArm(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false)
    {
        if (parent == null)
        {
            Main.Instance.Log($"Can't create new arm; parent is null", OWML.Common.MessageType.Debug);
            return null;
        }
        if (parent.transform.Find("Arm") != null)
        {
            Main.Instance.Log($"{parent.name} already has an arm. Replacing it.", OWML.Common.MessageType.Debug);
            Destroy(parent.transform.Find("Arm").gameObject);
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

        MeshRenderer noSuitMeshRenderer = noSuit.GetComponent<MeshRenderer>();
        MeshRenderer suitMeshRenderer = suit.GetComponent<MeshRenderer>();
        foreach (Material material in noSuitMeshRenderer.materials)
        {
            material.renderQueue = parent.GetComponent<MeshRenderer>().material.renderQueue;
            if (!useDefaultShader)
            {
                material.shader = parent.GetComponent<MeshRenderer>().material.shader;
            }
        }
        suitMeshRenderer.material.renderQueue = noSuitMeshRenderer.material.renderQueue;
        suitMeshRenderer.material.shader = noSuitMeshRenderer.material.shader;

        return arm;
    }
}
using UnityEngine;

namespace Immersion.Components;

public class ToolArmHandler
{
    public static GameObject NewArmPrefab(Transform parent, Vector3 localPos, Quaternion localRot, Vector3 scale, bool useDefaultShader = false)
    {
        if (parent == null)
        {
            Main.Instance.WriteLine($"Can't create new arm prefab; parent is null", OWML.Common.MessageType.Debug);
            return null;
        }
        if (parent.GetComponent<ToolArm>() != null)
        {
            Main.Instance.WriteLine($"{parent.name} already has an arm. Replacing it.", OWML.Common.MessageType.Debug);
            GameObject.Destroy(parent.GetComponent<ToolArm>().gameObject);
        }

        GameObject arm = new("ViewmodelArm");
        arm.transform.parent = parent;
        arm.transform.localPosition = localPos;
        arm.transform.localRotation = localRot;
        arm.transform.localScale = scale;

        GameObject noSuit = GameObject.Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit"));
        noSuit.name = "NoSuit";
        noSuit.transform.parent = arm.transform;
        noSuit.layer = 27;
        noSuit.transform.localPosition = Vector3.zero;
        noSuit.transform.localRotation = Quaternion.Euler(330f, 0f, 300f);
        noSuit.transform.localScale = Vector3.one;

        GameObject suit = GameObject.Instantiate(GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm"));
        suit.name = "Suit";
        suit.transform.parent = arm.transform;
        suit.layer = 27;
        suit.transform.localPosition = new Vector3(-0.02f, 0.03f, 0.02f);
        suit.transform.localRotation = Quaternion.Euler(330f, 0f, 300f);
        suit.transform.localScale = Vector3.one;

        MeshRenderer noSuitMeshRenderer = noSuit.GetComponent<MeshRenderer>();
        MeshRenderer suitMeshRenderer = suit.GetComponent<MeshRenderer>();
        noSuitMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        suitMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
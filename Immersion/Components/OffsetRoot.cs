using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

public class OffsetRoot : MonoBehaviour
{
    private (Vector3 position, Quaternion rotation) currentOffset;

    private (Vector3 position, Quaternion rotation) nextOffset;

    public static OffsetRoot NewOffsetRoot(string name, GameObject gameObject)
    {
        var offsetRoot = new GameObject(name).AddComponent<OffsetRoot>();
        offsetRoot.transform.parent = gameObject.transform.parent;
        offsetRoot.transform.localPosition = Vector3.zero;
        offsetRoot.transform.localEulerAngles = Vector3.zero;
        gameObject.transform.parent = offsetRoot.transform;
        return offsetRoot;
    }

    /// <summary>
    /// Adds a translational offset to be applied on the next LateUpdate
    /// </summary>
    /// <param name="position">The translational component of the offset</param>
    public virtual void AddOffset(Vector3 position)
    {
        nextOffset.position += position;
    }

    /// <summary>
    /// Adds a rotational offset to be applied on the next LateUpdate
    /// </summary>
    /// <param name="rotation">The rotational component of the offset</param>
    public virtual void AddOffset(Quaternion rotation)
    {
        nextOffset.rotation *= rotation;
    }

    /// <summary>
    /// Adds an offset to be applied on the next LateUpdate
    /// </summary>
    /// <param name="position">The translational component of the offset</param>
    /// <param name="rotation">The rotational component of the offset</param>
    public void AddOffset(Vector3 position, Quaternion rotation)
    {
        AddOffset(position);
        AddOffset(rotation);
    }

    private void ApplyOffset()
    {
        ResetOffset();
        currentOffset = nextOffset;
        transform.localPosition += currentOffset.position;
        transform.localRotation *= currentOffset.rotation;
        nextOffset = (Vector3.zero, Quaternion.identity);
    }

    private void ResetOffset()
    {
        transform.localPosition -= currentOffset.position;
        transform.localRotation *= Quaternion.Inverse(currentOffset.rotation);
        currentOffset = (Vector3.zero, Quaternion.identity);
    }

    private void LateUpdate()
    {
        ApplyOffset();
    }
}
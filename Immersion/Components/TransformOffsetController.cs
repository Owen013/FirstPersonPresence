using UnityEngine;

namespace Immersion.Components;

public abstract class TransformOffsetController : MonoBehaviour
{
    protected (Vector3 position, Quaternion rotation) currentOffset;

    protected (Vector3 position, Quaternion rotation) nextOffset;

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
    public virtual void AddOffset(Vector3 position, Quaternion rotation)
    {
        AddOffset(position);
        AddOffset(rotation);
    }

    protected virtual void ApplyOffset()
    {
        ResetOffset();
        currentOffset = nextOffset;
        transform.localPosition += currentOffset.position;
        transform.localRotation *= currentOffset.rotation;
        nextOffset = (Vector3.zero, Quaternion.identity);
    }

    protected virtual void ResetOffset()
    {
        transform.localPosition -= currentOffset.position;
        transform.localRotation *= Quaternion.Inverse(currentOffset.rotation);
        currentOffset = (Vector3.zero, Quaternion.identity);
    }

    protected virtual void LateUpdate()
    {
        ApplyOffset();
    }
}
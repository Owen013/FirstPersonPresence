using UnityEngine;

namespace Immersion.Components;

class OffsetRoot : MonoBehaviour
{
    GameObject offsetObject;

    Vector3 nextLocalPosition;

    Quaternion nextLocalRotation;

    /// <summary>
    /// Creates a new OffsetRoot for the specified GameObject.
    /// </summary>
    /// <param name="name">The name of the OffsetRoot GameObject.</param>
    /// <param name="offsetObject">The GameObject that this OffsetRoot is offsetting.</param>
    /// <returns>The new OffsetRoot.</returns>
    public static OffsetRoot NewOffsetRoot(GameObject offsetObject)
    {
        var offsetRoot = new GameObject().AddComponent<OffsetRoot>();
        offsetRoot.transform.parent = offsetObject.transform.parent;
        offsetRoot.transform.localPosition = Vector3.zero;
        offsetRoot.transform.localEulerAngles = Vector3.zero;
        offsetRoot.offsetObject = offsetObject;
        offsetObject.transform.parent = offsetRoot.transform;
        return offsetRoot;
    }

    /// <summary>
    /// Adds a translational offset to be applied on the next LateUpdate.
    /// </summary>
    /// <param name="position">The translational component of the offset.</param>
    public void AddOffset(Vector3 position)
    {
        nextLocalPosition += position;
    }

    /// <summary>
    /// Adds a rotational offset to be applied on the next LateUpdate.
    /// </summary>
    /// <param name="rotation">The rotational component of the offset.</param>
    public void AddOffset(Quaternion rotation)
    {
        nextLocalRotation *= rotation;
    }

    /// <summary>
    /// Adds an offset to be applied on the next LateUpdate.
    /// </summary>
    /// <param name="position">The translational component of the offset.</param>
    /// <param name="rotation">The rotational component of the offset.</param>
    public void AddOffset(Vector3 position, Quaternion rotation)
    {
        AddOffset(position);
        AddOffset(rotation);
    }

    void ResetOffset()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void ApplyOffset()
    {
        ResetOffset();

        transform.localPosition = nextLocalPosition;
        nextLocalRotation.ToAngleAxis(out float angle, out Vector3 axis);
        transform.RotateAround(offsetObject.transform.position, offsetObject.transform.TransformDirection(axis), angle);

        nextLocalPosition = Vector3.zero;
        nextLocalRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        ApplyOffset();
    }
}
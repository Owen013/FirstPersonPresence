using UnityEngine;

namespace Immersion.Interfaces
{
    public interface ISmolHatchling
    {
        public Vector3 GetCurrentScale();

        public Vector3 GetTargetScale();

        public float GetAnimSpeed();
    }
}
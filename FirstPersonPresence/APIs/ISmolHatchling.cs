using UnityEngine;

namespace FirstPersonPresence.APIs
{
    public interface ISmolHatchling
    {
        public Vector3 GetCurrentScale();

        public Vector3 GetTargetScale();

        public float GetAnimSpeed();
    }
}
using UnityEngine;

namespace Immersion.Components
{
    public abstract class ToggleableBehaviour : MonoBehaviour
    {
        protected abstract void OnConfigured();

        protected virtual void Awake()
        {
            Config.OnConfigured += OnConfigured;
            OnConfigured();
        }

        protected virtual void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public abstract class ToggleableBehaviour : MonoBehaviour
    {
        protected abstract void UpdateEnabled();

        protected virtual void Awake()
        {
            Config.OnConfigured += UpdateEnabled;
            UpdateEnabled();
        }

        protected virtual void OnDestroy()
        {
            Config.OnConfigured -= UpdateEnabled;
        }
    }
}
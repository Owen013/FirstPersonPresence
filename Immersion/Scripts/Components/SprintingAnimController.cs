using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class SprintingAnimController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private float _strength;

        private float _velocity;

        private void OnConfigured()
        {
            enabled = Config.EnableSprintingAnim;
        }

        private void Awake()
        {
            _offsetManager = OffsetManager.Instance;
            Config.OnConfigured += OnConfigured;
            OnConfigured();
        }

        private void Update()
        {
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime != 0f)
            {
                _strength = Mathf.SmoothDamp(_strength, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _velocity, 0.2f, Mathf.Infinity, deltaTime);
            }

            _offsetManager.AddToolOffsets(Quaternion.Euler(15f * _strength, 0f, 0f));
        }

        private void OnDisable()
        {
            _strength = 0f;
            _velocity = 0f;
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class SprintingAnimController : ToggleableBehaviour
    {
        private OffsetManager _offsetManager;

        private float _strength;

        private float _velocity;

        protected override void OnConfigured()
        {
            enabled = Config.EnableSprintingAnim;
        }

        protected override void Awake()
        {
            base.Awake();
            _offsetManager = OffsetManager.Instance;
        }

        private void Update()
        {
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime != 0f)
            {
                _strength = Mathf.SmoothDamp(_strength, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _velocity, 0.2f, Mathf.Infinity, deltaTime);
            }

            _offsetManager.AddToolOffset(Quaternion.Euler(15f * _strength, 0f, 0f), Tools.All);
        }

        private void OnDisable()
        {
            _strength = 0f;
            _velocity = 0f;
        }
    }
}
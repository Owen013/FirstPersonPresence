using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class SprintingAnimController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private float _sprintAnimScale;

        private float _sprintAnimDampVel;

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
                _sprintAnimScale = Mathf.SmoothDamp(_sprintAnimScale, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _sprintAnimDampVel, 0.2f, Mathf.Infinity, deltaTime);
            }

            _offsetManager.AddToolOffsets(Quaternion.Euler(15f * _sprintAnimScale, 0f, 0f));
        }

        private void OnDisable()
        {
            _sprintAnimScale = 0f;
            _sprintAnimDampVel = 0f;
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
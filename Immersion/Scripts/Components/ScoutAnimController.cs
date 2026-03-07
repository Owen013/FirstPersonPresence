using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ScoutAnimController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private float _lastScoutLaunchTime;

        private bool _isActive;

        private float _strength;

        private float _velocity;

        private void OnConfigured()
        {
            enabled = Config.EnableScoutAnim;
        }

        private void Awake()
        {
            _offsetManager = OffsetManager.Instance;
            Config.OnConfigured += OnConfigured;
            OnConfigured();

            Locator.GetToolModeSwapper().GetProbeLauncher().OnLaunchProbe += (_) =>
            {
                // play scout launcher animation if enabled
                if (Config.EnableScoutAnim)
                {
                    _isActive = true;
                    _lastScoutLaunchTime = Time.time;
                }
            };
        }

        private void Update()
        {
            if (_isActive)
            {
                float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
                if (deltaTime != 0f)
                {
                    float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
                    // damp moves quickly during the initial recoil, and slowly during the recovery
                    float dampTime = targetRecoil > _strength ? 0.05f : 0.1f;
                    _strength = Mathf.SmoothDamp(_strength, targetRecoil, ref _velocity, dampTime, Mathf.Infinity, deltaTime);
                }

                if (_strength != 0f)
                {
                    // apply recoils to camera and scout launcher
                    _offsetManager.AddCameraOffset(Quaternion.Euler(_strength * new Vector3(-5f, 0f, -5f)));
                    _offsetManager.ProbeLauncherOffsetRoot.AddOffset(new Vector3(0.25f, -0.25f, -0.5f) * _strength, Quaternion.Euler(new Vector3(-15f, 0f, -15f) * _strength));
                }
                else
                {
                    _isActive = false;
                }
            }
        }

        private void OnDisable()
        {
            // reset recoil parameters if disabled
            _isActive = false;
            _strength = 0f;
            _velocity = 0f;
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
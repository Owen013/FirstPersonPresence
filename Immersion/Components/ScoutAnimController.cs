using UnityEngine;

namespace Immersion.Components;

public class ScoutAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private float _lastScoutLaunchTime;

    private bool _isScoutAnimActive;

    private float _scoutAnimStrength;

    private float _scoutAnimVel;

    private void OnConfigured() =>
        enabled = Config.EnableScoutAnim;

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
                _isScoutAnimActive = true;
                _lastScoutLaunchTime = Time.time;
            }
        };
    }

    private void Update()
    {
        if (_isScoutAnimActive)
        {
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime != 0f)
            {
                float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
                // damp moves quickly during the initial recoil, and slowly during the recovery
                float dampTime = targetRecoil > _scoutAnimStrength ? 0.05f : 0.1f;
                _scoutAnimStrength = Mathf.SmoothDamp(_scoutAnimStrength, targetRecoil, ref _scoutAnimVel, dampTime, Mathf.Infinity, deltaTime);
            }

            if (_scoutAnimStrength != 0f)
            {
                // apply recoils to camera and scout launcher
                _offsetManager.AddCameraOffset(Quaternion.Euler(_scoutAnimStrength * new Vector3(-5f, 0f, -5f)));
                _offsetManager.ProbeLauncherOffsetRoot.AddOffset(new Vector3(0.25f, -0.25f, -0.5f) * _scoutAnimStrength, Quaternion.Euler(new Vector3(-15f, 0f, -15f) * _scoutAnimStrength));
            }
            else
                _isScoutAnimActive = false;
        }
    }

    private void OnDisable()
    {
        // reset recoil parameters if disabled
        _isScoutAnimActive = false;
        _scoutAnimStrength = 0f;
        _scoutAnimVel = 0f;
    }

    private void OnDestroy() =>
        Config.OnConfigured -= OnConfigured;
}
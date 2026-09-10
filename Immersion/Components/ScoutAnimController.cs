using UnityEngine;

namespace Immersion.Components;

public class ScoutAnimController : MonoBehaviour
{
    OffsetManager _offsetManager;

    float _lastScoutLaunchTime;

    bool _isAnimPlaying;

    float _animScale;

    float _animVelocity;

    void OnConfigured()
    {
        enabled = Config.EnableScoutAnim;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;

        Locator.GetToolModeSwapper().GetProbeLauncher().OnLaunchProbe += (_) =>
        {
            if (Config.EnableScoutAnim)
            {
                _isAnimPlaying = true;
                _lastScoutLaunchTime = Time.time;
            }
        };

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        if (_isAnimPlaying)
        {
            float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime != 0f)
            {
                float targetRecoil = Mathf.Max(_lastScoutLaunchTime + 0.5f - Time.time, 0f) * 2f;
                float dampTime = targetRecoil > _animScale ? 0.05f : 0.1f;
                _animScale = Mathf.SmoothDamp(_animScale, targetRecoil, ref _animVelocity, dampTime, Mathf.Infinity,
                                              deltaTime);
            }

            if (_animScale != 0f)
            {
                var cameraOffsetRot = Quaternion.Euler(_animScale * new Vector3(-5f, 0f, -5f));
                var probeLauncherOffsetPos = _animScale * new Vector3(0.1f, -0.1f, -0.2f);
                var probeLauncherOffsetRot = Quaternion.Euler(new Vector3(-15f, 0f, -15f) * _animScale);
                _offsetManager.AddCameraOffset(cameraOffsetRot);
                _offsetManager.AddToolOffset(probeLauncherOffsetPos, probeLauncherOffsetRot, Tools.ProbeLauncher);
            }
            else
                _isAnimPlaying = false;
        }
    }

    void OnDisable()
    {
        _isAnimPlaying = false;
        _animScale = 0f;
        _animVelocity = 0f;
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
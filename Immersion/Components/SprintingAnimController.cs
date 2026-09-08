using UnityEngine;
using static Immersion.ModMain;

namespace Immersion.Components;

class SprintingAnimController : MonoBehaviour
{
    OffsetManager _offsetManager;

    float _animStrength;

    float _animVelocity;

    void OnConfigured()
    {
        enabled = Config.EnableSprintingAnim;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f)
        {
            float targetStrength = HikersModAPI.IsSprinting() ? 1f : 0f;
            _animStrength = Mathf.SmoothDamp(_animStrength, targetStrength, ref _animVelocity, 0.2f, Mathf.Infinity,
                                             deltaTime);
        }

        _offsetManager.AddToolOffset(Quaternion.Euler(15f * _animStrength, 0f, 0f), Tools.All);
    }

    void OnDisable()
    {
        _animStrength = 0f;
        _animVelocity = 0f;
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
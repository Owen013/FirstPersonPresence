using UnityEngine;
using static Immersion.Immersion;

namespace Immersion.Components;

public class SprintingAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private float _animStrength;

    private float _animVelocity;

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
            float targetStrength = HikersModAPI.IsSprinting() ? 1f : 0f;
            _animStrength = Mathf.SmoothDamp(_animStrength, targetStrength, ref _animVelocity, 0.2f, Mathf.Infinity, deltaTime);
        }

        _offsetManager.AddToolOffset(Quaternion.Euler(15f * _animStrength, 0f, 0f), Tools.All);
    }

    private void OnDisable()
    {
        _animStrength = 0f;
        _animVelocity = 0f;
    }

    private void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
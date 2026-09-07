using UnityEngine;

namespace Immersion.Components;

class SprintingAnimController : ToggleableBehaviour
{
    OffsetManager _offsetManager;

    float _animStrength;

    float _animVelocity;

    protected override void OnConfigured()
    {
        enabled = Config.EnableSprintingAnim;
    }

    protected override void Awake()
    {
        base.Awake();
        _offsetManager = OffsetManager.Instance;
    }

    void Update()
    {
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f)
            _animStrength = Mathf.SmoothDamp(_animStrength, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _animVelocity, 0.2f, Mathf.Infinity, deltaTime);

        _offsetManager.AddToolOffset(Quaternion.Euler(15f * _animStrength, 0f, 0f), Tools.All);
    }

    void OnDisable()
    {
        _animStrength = 0f;
        _animVelocity = 0f;
    }
}
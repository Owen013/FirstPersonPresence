using UnityEngine;

namespace Immersion.Components;

public class BreathingAnimController : ToggleableBehaviour
{
    OffsetManager _offsetManager;

    Vector3 _animPosition;

    Vector3 _animTargetPosition;

    Vector3 _animVelocity;

    float _nextUpdateTime;

    float MaxDisplacement => 0.005f * Config.BreathingAnimScale;

    protected override void OnConfigured()
    {
        enabled = Config.EnableBreathingAnim;
    }

    protected override void Awake()
    {
        base.Awake();
        _offsetManager = OffsetManager.Instance;
    }

    void Update()
    {
        // if delta time is zero when using SmoothDamp, all hell breaks loose, so don't do it
        if (Time.deltaTime != 0f)
        {
            _animPosition = Vector3.SmoothDamp(_animPosition, _animTargetPosition, ref _animVelocity, 2f);
        }

        _offsetManager.AddToolOffset(MaxDisplacement * _animPosition, Tools.All);

        if (Time.time >= _nextUpdateTime)
        {
            // choose random tool offset
            _animTargetPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            _nextUpdateTime = Time.time + Random.Range(0.1f, 1f);
        }
    }
}
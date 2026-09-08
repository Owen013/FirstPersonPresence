using UnityEngine;

namespace Immersion.Components;

class BreathingAnimController : MonoBehaviour
{
    OffsetManager _offsetManager;

    Vector3 _animPosition;

    Vector3 _animTargetPosition;

    Vector3 _animVelocity;

    float _nextUpdateTime;

    void OnConfigured()
    {
        enabled = Config.EnableBreathingAnim;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        if (Time.deltaTime != 0f)
            _animPosition = Vector3.SmoothDamp(_animPosition, _animTargetPosition, ref _animVelocity, 2f);

        float animScale = 0.005f * Config.BreathingAnimScale;
        _offsetManager.AddToolOffset(animScale * _animPosition, Tools.All);

        if (Time.time >= _nextUpdateTime)
        {
            _animTargetPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            _nextUpdateTime = Time.time + Random.Range(0.1f, 1f);
        }
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
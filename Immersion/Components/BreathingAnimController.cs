using UnityEngine;

namespace Immersion.Components;

public class BreathingAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private Vector3 _animPosition;

    private Vector3 _animTargetPosition;

    private Vector3 _animVelocity;

    private float _nextUpdateTime;

    private void OnConfigured()
    {
        enabled = Config.EnableBreathingAnim;
    }

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    private void Update()
    {
        if (Time.deltaTime != 0f)
        {
            _animPosition = Vector3.SmoothDamp(_animPosition, _animTargetPosition, ref _animVelocity, 2f);
        }

        float animScale = 0.005f * Config.BreathingAnimScale;
        _offsetManager.AddToolOffset(animScale * _animPosition, Tools.All);

        if (Time.time >= _nextUpdateTime)
        {
            _animTargetPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
            _nextUpdateTime = Time.time + Random.Range(0.1f, 1f);
        }
    }

    private void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
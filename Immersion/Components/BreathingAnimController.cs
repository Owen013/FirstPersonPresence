using UnityEngine;

namespace Immersion.Components;

public class BreathingAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private Vector3 _breathingAnimPos;

    private Vector3 _breathingAnimTargetPos;

    private Vector3 _breathingAnimDampVel;

    private float _breathingAnimNextUpdateTime;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
    }

    private void Update()
    {
        if (Config.EnableBreathingAnim && Config.BreathingAnimStrength != 0f)
        {
            if (Time.deltaTime != 0f)
                _breathingAnimPos = Vector3.SmoothDamp(_breathingAnimPos, _breathingAnimTargetPos, ref _breathingAnimDampVel, 1f);

            _offsetManager.AddToolOffsets(Config.BreathingAnimStrength * 0.005f * _breathingAnimPos);

            if (Time.time >= _breathingAnimNextUpdateTime)
            {
                // choose random tool offset
                _breathingAnimTargetPos = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                _breathingAnimNextUpdateTime = Time.time + Random.Range(0.1f, 1f);
            }
        }
        else
        {
            _breathingAnimPos = Vector3.zero;
            _breathingAnimTargetPos = Vector3.zero;
            _breathingAnimDampVel = Vector3.zero;
            _breathingAnimNextUpdateTime = 0f;
        }
    }
}

using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class BreathingAnimController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private Vector3 _breathingAnimPos;

        private Vector3 _breathingAnimTargetPos;

        private Vector3 _breathingAnimDampVel;

        private float _breathingAnimNextUpdateTime;

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
            // if delta time is zero when using SmoothDamp, all hell breaks loose, so don't do it
            if (Time.deltaTime != 0f)
            {
                _breathingAnimPos = Vector3.SmoothDamp(_breathingAnimPos, _breathingAnimTargetPos, ref _breathingAnimDampVel, 1f);
            }

            _offsetManager.AddToolOffsets(Config.BreathingAnimStrength * 0.005f * _breathingAnimPos);

            if (Time.time >= _breathingAnimNextUpdateTime)
            {
                // choose random tool offset
                _breathingAnimTargetPos = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                _breathingAnimNextUpdateTime = Time.time + Random.Range(0.1f, 1f);
            }
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
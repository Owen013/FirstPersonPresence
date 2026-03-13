using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class BreathingAnimController : ToggleableBehaviour
    {
        private OffsetManager _offsetManager;

        private Vector3 _position;

        private Vector3 _targetPosition;

        private Vector3 _velocity;

        private float _nextUpdateTime;

        private float MaxDisplacement => 0.005f * Config.BreathingAnimScale;

        protected override void UpdateEnabled()
        {
            enabled = Config.EnableBreathingAnim;
        }

        protected override void Awake()
        {
            base.Awake();
            _offsetManager = OffsetManager.Instance;
        }

        private void Update()
        {
            // if delta time is zero when using SmoothDamp, all hell breaks loose, so don't do it
            if (Time.deltaTime != 0f)
            {
                _position = Vector3.SmoothDamp(_position, _targetPosition, ref _velocity, 2f);
            }

            _offsetManager.AddToolOffsets(MaxDisplacement * _position);

            if (Time.time >= _nextUpdateTime)
            {
                // choose random tool offset
                _targetPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
                _nextUpdateTime = Time.time + Random.Range(0.1f, 1f);
            }
        }
    }
}
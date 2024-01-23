using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonPresence.Components
{
    public class StepCounter : MonoBehaviour
    {
        private PlayerAnimController _animController;
        private List<float> _steps;
        private float _stepKeepTime = 5f;
        private float _lastLogTime;
        private float _logIncrement = 5f;

        private void Awake()
        {
            _animController = GetComponent<PlayerAnimController>();
            _animController.OnRightFootGrounded += CountStep;
        }

        private void Update()
        {
            if (_steps == null) return;

            for (int i = 0; i < _steps.Count; i++)
            {
                if (Time.time - _steps[i] > _stepKeepTime) _steps.Remove(_steps[i]);
            }

            if (Time.time - _lastLogTime >= _logIncrement)
            {
                _lastLogTime = Time.time;
                Main.Instance.DebugLog($"{_steps.Count} steps in last {_stepKeepTime} seconds, {_steps.Count / _stepKeepTime} per anim cycle");
            }
        }

        private void CountStep()
        {
            if (_steps == null)
            {
                _steps = new List<float>();
            }
            _steps.Add(Time.time);
        }
    }
}
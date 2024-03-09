using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstPersonPresence.Components
{
    public class StepCounter : MonoBehaviour
    {
        private PlayerAnimController _animController;
        private List<float> _steps;
        private float _stepKeepTime = 5f;

        private void Awake()
        {
            _animController = GetComponent<PlayerAnimController>();
            _steps = new List<float>();
            _animController.OnRightFootGrounded += CountStep;
        }

        private void Update()
        {
            if (Keyboard.current[Key.R].wasPressedThisFrame)
            {
                _steps.Clear();
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (Time.time - _steps[i] > _stepKeepTime) _steps.Remove(_steps[i]);
            }
        }

        private void CountStep()
        {
            _steps.Add(Time.time);
            Main.Instance.DebugLog($"{_steps.Count} steps in last {_stepKeepTime} seconds, {_steps.Count / _stepKeepTime} per anim cycle");
        }
    }
}
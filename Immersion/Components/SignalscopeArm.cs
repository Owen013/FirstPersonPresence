using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Immersion.Components
{
    public class SignalscopeArm : ToolArm
    {
        protected new virtual void Start()
        {
            base.Start();
            _suitArm.transform.localPosition = new Vector3(0f, -0.05f, 0.05f);
            _suitArm.transform.localRotation = Quaternion.Euler(0f, 350f, 330f);
        }
    }
}
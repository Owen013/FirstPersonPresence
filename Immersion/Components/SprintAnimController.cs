using UnityEngine;

namespace Immersion.Components;

public class SprintAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private float _sprintAnimScale;

    private float _sprintAnimDampVel;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
    }
    private void Update()
    {
        if (Config.EnableSprintingAnim)
        {
            if (Time.deltaTime != 0f)
                _sprintAnimScale = Mathf.SmoothDamp(_sprintAnimScale, ModMain.HikersModAPI.IsSprinting() ? 1f : 0f, ref _sprintAnimDampVel, 0.2f);
            _offsetManager.AddToolOffsets(Quaternion.Euler(15f * _sprintAnimScale, 0f, 0f));
        }
        else
        {
            _sprintAnimScale = 0f;
            _sprintAnimDampVel = 0f;
        }
    }
}

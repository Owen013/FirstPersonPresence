using UnityEngine;
using static Immersion.ModMain;

namespace Immersion.Components;

public class ViewbobController : MonoBehaviour
{
    OffsetManager _offsetManager;

    PlayerCharacterController _playerController;

    PlayerAnimController _animController;

    float _timePosition;

    float _strength;

    float _velocity;

    void OnConfigured()
    {
        enabled = Config.EnableHeadBob || Config.EnableViewmodelBob;
    }

    void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _playerController = Locator.GetPlayerController();
        _animController = _playerController.GetComponentInChildren<PlayerAnimController>();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        if (!_playerController._isMovementLocked)
        {
            if (OWTime.IsPaused(OWTime.PauseType.Reading))
                _strength = Mathf.SmoothDamp(_strength, 0f, ref _velocity, 0.05f, Mathf.Infinity,
                                             Time.unscaledDeltaTime);
            else if (Time.deltaTime != 0f)
            {
                _timePosition += _animController._animator.speed * Time.deltaTime;
                if (_playerController.IsGrounded())
                {
                    Vector3 groundVel = _playerController.GetRelativeGroundVelocity();
                    groundVel.y = 0f;
                    if (Mathf.Abs(groundVel.x) < 0.05f)
                        groundVel.x = 0f;
                    if (Mathf.Abs(groundVel.z) < 0.05f)
                        groundVel.z = 0f;
                    groundVel /= SmolHatchlingAPI?.GetPlayerScale() ?? 1f;

                    float targetStrength = Mathf.Min(groundVel.magnitude / 6f, 2f);
                    float smoothTime = 0.05f;
                    _strength = Mathf.SmoothDamp(_strength, targetStrength, ref _velocity, smoothTime);
                }
                else
                    _strength = Mathf.SmoothDamp(_strength, 0f, ref _velocity, 1f);
            }

            Vector2 viewBob = new()
            {
                x = _strength * Mathf.Sin(_timePosition * 2f * Mathf.PI),
                y = _strength * Mathf.Cos(_timePosition * 4f * Mathf.PI)
            };
            if (Config.EnableHeadBob)
            {
                float headBobScale = 0.02f * Config.HeadBobScale;
                _offsetManager.AddCameraOffset(headBobScale * new Vector3(viewBob.x, viewBob.y));
            }

            if (Config.EnableViewmodelBob)
            {
                float viewmodelBobScale = 0.02f * Config.ViewmodelBobScale;
                var offsetPos = viewmodelBobScale * new Vector3(viewBob.x, 0.15f * viewBob.y);

                float maxViewmodelBobAngle = 0.75f * Config.ViewmodelBobScale;
                float offsetAngle = maxViewmodelBobAngle * _strength * -Mathf.Sin(_timePosition * 4f * Mathf.PI);

                _offsetManager.AddToolOffset(offsetPos, Quaternion.AngleAxis(offsetAngle, Vector3.right), Tools.All);
            }
        }
        else
        {
            _timePosition = 0f;
            _strength = 0f;
            _velocity = 0f;
        }
    }

    void OnDisable()
    {
        _timePosition = 0f;
        _strength = 0f;
        _velocity = 0f;
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
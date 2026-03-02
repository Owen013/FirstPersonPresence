using UnityEngine;

namespace Immersion.Scripts.Components;

public class ViewbobController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCharacterController _playerController;

    private PlayerAnimController _animController;

    private float _viewbobTime;

    private float _viewbobScale;

    private float _viewbobDampVel;

    private void OnConfigured() =>
        enabled = Config.EnableHeadBob || Config.EnableViewmodelBob;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _playerController = Locator.GetPlayerController();
        _animController = _playerController.GetComponentInChildren<PlayerAnimController>();

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    private void Update()
    {
        if (!_playerController._isMovementLocked)
        {
            // if time is frozen during gameplay, smoothly transition viewbob strength to zero
            if (OWTime.IsPaused(OWTime.PauseType.Reading))
                _viewbobScale = Mathf.SmoothDamp(_viewbobScale, 0f, ref _viewbobDampVel, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime);
            else if (Time.deltaTime != 0f)
            {
                // viewbob cycle increases based on player ground speed
                // viewbob time and viewbob strength are used by both camera and tool bobbing
                _viewbobTime += _animController._animator.speed * Time.deltaTime;
                if (_playerController.IsGrounded())
                {
                    // change viewbob strength quickly if on ground
                    Vector3 groundVel = _playerController.GetRelativeGroundVelocity();
                    groundVel.y = 0f;
                    if (Mathf.Abs(groundVel.x) < 0.05f)
                        groundVel.x = 0f;
                    if (Mathf.Abs(groundVel.z) < 0.05f)
                        groundVel.z = 0f;

                    if (ModMain.SmolHatchlingAPI != null)
                    {
                        // avoid dividing by 0
                        float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                        if (playerScale != 0f)
                            groundVel /= ModMain.SmolHatchlingAPI.GetPlayerScale();
                    }

                    _viewbobScale = Mathf.SmoothDamp(_viewbobScale, Mathf.Min(groundVel.magnitude / 6f, 2f), ref _viewbobDampVel, 0.05f);
                }
                else
                {
                    // decay viewbob strength slowly if in air
                    _viewbobScale = Mathf.SmoothDamp(_viewbobScale, 0f, ref _viewbobDampVel, 1f);
                }
            }

            // trig is used for a circular viewbob motion
            var viewBob = _viewbobScale * new Vector2(Mathf.Sin(_viewbobTime * 2f * Mathf.PI), Mathf.Cos(_viewbobTime * 4f * Mathf.PI));

            // apply camera offset if camera bob is enabled
            if (Config.EnableHeadBob)
                _offsetManager.AddCameraOffset(Config.HeadBobStrength * 0.02f * new Vector3(viewBob.x, viewBob.y));

            // apply tool offset if tool bob is enabled
            if (Config.EnableViewmodelBob)
            {
                var offsetPos = Config.ViewmodelBobStrength * new Vector3(0.02f * viewBob.x, 0.003f * viewBob.y);
                var offsetRot = Quaternion.Euler(Config.ViewmodelBobStrength * _viewbobScale * -0.75f * Mathf.Sin(_viewbobTime * 4f * Mathf.PI), 0f, 0f);
                _offsetManager.AddToolOffsets(offsetPos, offsetRot);
            }
        }
        else
        {
            _viewbobTime = 0f;
            _viewbobScale = 0f;
            _viewbobDampVel = 0f;
        }
    }

    private void OnDisable()
    {
        _viewbobTime = 0f;
        _viewbobScale = 0f;
        _viewbobDampVel = 0f;
    }

    private void OnDestroy() =>
        Config.OnConfigured -= OnConfigured;
}
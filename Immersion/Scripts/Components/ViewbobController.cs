using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewbobController : MonoBehaviour
    {
        private OffsetManager _offsetManager;

        private PlayerCharacterController _playerController;

        private PlayerAnimController _animController;

        private float _timePosition;

        private float _strength;

        private float _velocity;

        private float MaxHeadBobDisplacement => 0.02f * Config.HeadBobScale;

        private float MaxViewmodelBobDisplacement => 0.02f * Config.ViewmodelBobScale;

        private float MaxViewmodelBobAngle => 0.75f * Config.ViewmodelBobScale;

        private void OnConfigured()
        {
            enabled = Config.EnableHeadBob || Config.EnableViewmodelBob;
        }

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
                {
                    _strength = Mathf.SmoothDamp(_strength, 0f, ref _velocity, 0.05f, Mathf.Infinity, Time.unscaledDeltaTime);
                }
                else if (Time.deltaTime != 0f)
                {
                    // viewbob cycle increases based on player ground speed
                    // viewbob time and viewbob strength are used by both camera and tool bobbing
                    _timePosition += _animController._animator.speed * Time.deltaTime;
                    if (_playerController.IsGrounded())
                    {
                        // change viewbob strength quickly if on ground
                        Vector3 groundVel = _playerController.GetRelativeGroundVelocity();
                        groundVel.y = 0f;
                        if (Mathf.Abs(groundVel.x) < 0.05f)
                        {
                            groundVel.x = 0f;
                        }
                        if (Mathf.Abs(groundVel.z) < 0.05f)
                        {
                            groundVel.z = 0f;
                        }

                        if (ModMain.SmolHatchlingAPI != null)
                        {
                            // avoid dividing by 0
                            float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                            if (playerScale != 0f)
                            {
                                groundVel /= ModMain.SmolHatchlingAPI.GetPlayerScale();
                            }
                        }

                        _strength = Mathf.SmoothDamp(_strength, Mathf.Min(groundVel.magnitude / 6f, 2f), ref _velocity, 0.05f);
                    }
                    else
                    {
                        // decay viewbob strength slowly if in air
                        _strength = Mathf.SmoothDamp(_strength, 0f, ref _velocity, 1f);
                    }
                }

                // trig is used for a circular viewbob motion
                var viewBob = _strength * new Vector2(Mathf.Sin(_timePosition * 2f * Mathf.PI), Mathf.Cos(_timePosition * 4f * Mathf.PI));

                // apply camera offset if camera bob is enabled
                if (Config.EnableHeadBob)
                {
                    _offsetManager.AddCameraOffset(MaxHeadBobDisplacement * new Vector3(viewBob.x, viewBob.y));
                }

                // apply tool offset if tool bob is enabled
                if (Config.EnableViewmodelBob)
                {
                    var offsetPos = MaxViewmodelBobDisplacement * new Vector3(viewBob.x, 0.15f * viewBob.y);
                    float offsetAngle = MaxViewmodelBobAngle * _strength * -Mathf.Sin(_timePosition * 4f * Mathf.PI);
                    _offsetManager.AddToolOffsets(offsetPos, Quaternion.AngleAxis(offsetAngle, Vector3.right));
                }
            }
            else
            {
                _timePosition = 0f;
                _strength = 0f;
                _velocity = 0f;
            }
        }

        private void OnDisable()
        {
            _timePosition = 0f;
            _strength = 0f;
            _velocity = 0f;
        }

        private void OnDestroy()
        {
            Config.OnConfigured -= OnConfigured;
        }
    }
}
using UnityEngine;
using static Immersion.ModMain;

namespace Immersion.Components;

public class LandingAnimController : MonoBehaviour
{
    public static LandingAnimController Instance { get; private set; }

    public float AnimPos { get; private set; }

    OffsetManager _offsetManager;

    PlayerCharacterController _player;

    Vector3 _lastPlayerVelocity;

    bool _isCrouching;

    float _lastLandedSpeed;

    float _animVelocity;

    float MinPosition => -0.3f * Config.MaxLandingAnimDistance;

    public void UpdateLandingCrouchAnim(Animator playerAnimator)
    {
        if (enabled && Config.UseLandingCrouchAnim)
        {
            float crouchLayerWeight = playerAnimator.GetLayerWeight(1);
            float targetWeight = Mathf.Clamp01(AnimPos / -0.3f);
            playerAnimator.SetLayerWeight(1, Mathf.Max(crouchLayerWeight, targetWeight));
        }
    }

    void OnConfigured()
    {
        enabled = Config.EnableCameraLandingAnim || Config.EnableViewmodelLandingAnim;
    }

    void Awake()
    {
        Instance = this;
        _offsetManager = OffsetManager.Instance;
        _player = Locator.GetPlayerController();

        _player.OnBecomeGrounded += () =>
        {
            // If the player lands with a downward speed of at least 5, play landing anim.
            Vector3 groundContactPoint = _player.GetGroundContactPoint();
            Vector3 groundPointVelocity = _player.GetGroundBody().GetPointVelocity(groundContactPoint);
            Vector3 landingVelocity = groundPointVelocity - _lastPlayerVelocity;
            float landingSpeed = _player.transform.InverseTransformVector(landingVelocity).y;
            landingSpeed /= SmolHatchlingAPI?.GetPlayerScale() ?? 1f;
            if (landingSpeed >= 5f)
            {
                _lastLandedSpeed = landingSpeed;
                _isCrouching = true;
            }
        };

        Config.OnConfigured += OnConfigured;
        OnConfigured();
    }

    void Update()
    {
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f)
        {
            if (_isCrouching)
            {
                float playerScale = SmolHatchlingAPI?.GetPlayerScale() ?? 1f;
                AnimPos = Mathf.Min(AnimPos - _lastLandedSpeed * playerScale * deltaTime, 0f);
                if (AnimPos <= MinPosition)
                {
                    AnimPos = MinPosition;
                    _isCrouching = false;
                }
            }
            else
            {
                float smoothTime = 0.15f * Config.LandingAnimSmoothness;
                float maxSpeed = 1.5f * Config.MaxLandingAnimRecoverySpeed;
                AnimPos = Mathf.SmoothDamp(AnimPos, 0f, ref _animVelocity, smoothTime, maxSpeed, deltaTime);
            }
        }

        if (Config.EnableCameraLandingAnim)
            _offsetManager.AddCameraOffset(new Vector3(0f, AnimPos, 0f));
        if (Config.EnableViewmodelLandingAnim)
        {
            var cameraTransform = _offsetManager.transform;
            Vector3 toolOffsetDirection = cameraTransform.InverseTransformDirection(_player.transform.up);
            Vector3 offsetPosition = 0.1f * AnimPos * toolOffsetDirection;
            Quaternion offsetRotation = Quaternion.AngleAxis(_animVelocity, Vector3.right);
            _offsetManager.AddToolOffset(offsetPosition, Tools.All);
            _offsetManager.AddToolOffset(offsetRotation, Tools.All);
        }

        _lastPlayerVelocity = _player.GetAttachedOWRigidbody().GetVelocity();
    }

    void OnDisable()
    {
        _lastPlayerVelocity = Vector3.zero;
        AnimPos = 0f;
        _animVelocity = 0f;
        _isCrouching = false;
    }

    void OnDestroy()
    {
        Config.OnConfigured -= OnConfigured;
    }
}
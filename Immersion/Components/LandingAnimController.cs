using UnityEngine;

namespace Immersion.Components;

class LandingAnimController : ToggleableBehaviour
{
    public static LandingAnimController Instance { get; private set; }

    public float OffsetPosition { get; private set; }

    OffsetManager _offsetManager;

    PlayerCharacterController _playerController;

    Vector3 _lastPlayerVelocity;

    bool _isCrouching;

    float _lastLandedSpeed;

    float _animVelocity;

    float MinPosition => -0.3f * Config.MaxLandingAnimDistance;

    public void UpdateLandingCrouchAnim(Animator playerAnimator)
    {
        if (enabled && Config.UseLandingCrouchAnim)
            playerAnimator.SetLayerWeight(1, Mathf.Max(playerAnimator.GetLayerWeight(1), Mathf.Clamp01(OffsetPosition / -0.3f)));
    }

    protected override void OnConfigured()
    {
        enabled = Config.EnableCameraLandingAnim || Config.EnableViewmodelLandingAnim;
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        _offsetManager = OffsetManager.Instance;
        _playerController = Locator.GetPlayerController();

        _playerController.OnBecomeGrounded += () =>
        {
            // If the player lands with a downward speed of at least 5, play landing anim.
            Vector3 groundPointVelocity = _playerController.GetGroundBody().GetPointVelocity(_playerController.GetGroundContactPoint());
            Vector3 landingVelocity = groundPointVelocity - _lastPlayerVelocity;
            float landingSpeed = _playerController.transform.InverseTransformVector(landingVelocity).y;

            if (ModMain.SmolHatchlingAPI != null)
            {
                float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                if (playerScale != 0f)
                    landingSpeed /= ModMain.SmolHatchlingAPI.GetPlayerScale();
            }

            if (landingSpeed >= 5f)
            {
                _lastLandedSpeed = landingSpeed;
                _isCrouching = true;
            }
        };
    }

    void Update()
    {
        float deltaTime = OWTime.IsPaused(OWTime.PauseType.Reading) ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime != 0f)
        {
            if (_isCrouching)
            {
                float playerScale = ModMain.SmolHatchlingAPI != null ? ModMain.SmolHatchlingAPI.GetPlayerScale() : 1f;
                OffsetPosition = Mathf.Min(OffsetPosition - _lastLandedSpeed * playerScale * deltaTime, 0f);
                if (OffsetPosition <= MinPosition)
                {
                    OffsetPosition = MinPosition;
                    _isCrouching = false;
                }
            }
            else
                OffsetPosition = Mathf.SmoothDamp(OffsetPosition, 0f, ref _animVelocity, 0.15f * Config.LandingAnimSmoothness, 1.5f * Config.MaxLandingAnimRecoverySpeed, deltaTime);
        }

        if (Config.EnableCameraLandingAnim)
            _offsetManager.AddCameraOffset(new Vector3(0f, OffsetPosition, 0f));

        if (Config.EnableViewmodelLandingAnim)
        {
            var offsetPosition = 0.1f * OffsetPosition * _offsetManager.transform.InverseTransformDirection(_playerController.transform.up);
            var offsetRotation = Quaternion.Euler(_animVelocity, 0f, 0f);
            _offsetManager.AddToolOffset(offsetPosition, Tools.All);
            _offsetManager.AddToolOffset(offsetRotation, Tools.All);
        }

        _lastPlayerVelocity = _playerController.GetAttachedOWRigidbody().GetVelocity();
    }

    void OnDisable()
    {
        _lastPlayerVelocity = Vector3.zero;
        OffsetPosition = 0f;
        _animVelocity = 0f;
        _isCrouching = false;
    }
}
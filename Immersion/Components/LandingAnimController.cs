using UnityEngine;

namespace Immersion.Components;

public class LandingAnimController : MonoBehaviour
{
    public static LandingAnimController Instance { get; private set; }

    public float LandingAnimPosition { get; private set; }

    private OffsetManager _offsetManager;

    private PlayerCharacterController _playerController;

    private Vector3 _lastPlayerVel;

    private bool _isLandingAnimActive;

    private float _lastLandedSpeed;

    private float _landingAnimDampVel;

    private void OnConfigured() =>
        enabled = Config.EnableCameraLandingAnim || Config.EnableViewmodelLandingAnim;

    private void Awake()
    {
        Instance = this;

        _offsetManager = OffsetManager.Instance;
        _playerController = Locator.GetPlayerController();

        Config.OnConfigured += OnConfigured;
        OnConfigured();

        _playerController.OnBecomeGrounded += () =>
        {
            // if the player lands with a downward speed of at least 5, play landing anim
            Vector3 landingVel = _lastPlayerVel - _playerController.GetGroundBody().GetPointVelocity(_playerController.GetGroundContactPoint());
            float landingSpeed = -_playerController.transform.InverseTransformVector(landingVel).y;

            if (ModMain.SmolHatchlingAPI != null)
            {
                // avoid dividing by 0
                float playerScale = ModMain.SmolHatchlingAPI.GetPlayerScale();
                if (playerScale != 0f)
                    landingSpeed /= ModMain.SmolHatchlingAPI.GetPlayerScale();
            }

            if (landingSpeed >= 5f)
            {
                _lastLandedSpeed = landingSpeed;
                _isLandingAnimActive = true;
            }
        };
    }

    private void Update()
    {
        if (Time.deltaTime != 0f)
        {
            if (_isLandingAnimActive)
            {
                // update camera height based on landing speed
                float playerScale = ModMain.SmolHatchlingAPI != null ? ModMain.SmolHatchlingAPI.GetPlayerScale() : 1f;
                LandingAnimPosition = Mathf.Min(LandingAnimPosition - _lastLandedSpeed * playerScale * Time.deltaTime, 0f);
                if (LandingAnimPosition <= -0.3f * Config.MaxLandingAnimDistance)
                {
                    // landing anim bottoms out
                    LandingAnimPosition = -0.3f * Config.MaxLandingAnimDistance;
                    _isLandingAnimActive = false;
                }
            }
            else
                LandingAnimPosition = Mathf.SmoothDamp(LandingAnimPosition, 0f, ref _landingAnimDampVel, 0.15f * Config.LandingAnimRecoverySmoothness, 1.5f * Config.MaxLandingAnimRecoverySpeed);
        }

        // apply offsets
        if (Config.EnableCameraLandingAnim)
            _offsetManager.AddCameraOffset(new Vector3(0f, LandingAnimPosition, 0f));
        if (Config.EnableViewmodelLandingAnim)
        {
            _offsetManager.AddToolOffsets(0.1f * LandingAnimPosition * _offsetManager.transform.InverseTransformDirection(_playerController.transform.up));
            _offsetManager.AddToolOffsets(Quaternion.Euler(_landingAnimDampVel, 0f, 0f));
        }

        // keep track of player velocity
        _lastPlayerVel = _playerController.GetAttachedOWRigidbody().GetVelocity();
    }

    private void OnDisable()
    {
        // reset landing anim parameters if feature is disabled
        _lastPlayerVel = Vector3.zero;
        LandingAnimPosition = 0f;
        _landingAnimDampVel = 0f;
        _isLandingAnimActive = false;
    }

    private void OnDestroy() =>
        Config.OnConfigured -= OnConfigured;
}
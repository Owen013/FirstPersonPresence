using UnityEngine;

namespace Immersion.Components;

public class LandingAnimController : MonoBehaviour
{
    private OffsetManager _offsetManager;

    private PlayerCharacterController _playerController;

    private Vector3 _lastPlayerVel;

    private bool _isLandingAnimActive;

    private float _lastLandedSpeed;

    private float _landingAnimPos;

    private float _landingAnimDampVel;

    private void Awake()
    {
        _offsetManager = OffsetManager.Instance;
        _playerController = Locator.GetPlayerController();

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
        if (Config.EnableLandingAnim)
        {
            if (Time.deltaTime != 0f)
            {
                if (_isLandingAnimActive)
                {
                    // update camera height based on landing speed
                    float playerScale = ModMain.SmolHatchlingAPI != null ? ModMain.SmolHatchlingAPI.GetPlayerScale() : 1f;
                    _landingAnimPos = Mathf.Min(_landingAnimPos - _lastLandedSpeed * playerScale * Time.deltaTime, 0f);
                    if (_landingAnimPos <= -0.25f)
                    {
                        // landing anim bottoms out at -0.25
                        _landingAnimPos = -0.25f;
                        _isLandingAnimActive = false;
                    }
                }
                else
                {
                    _landingAnimPos = Mathf.SmoothDamp(_landingAnimPos, 0f, ref _landingAnimDampVel, 0.2f);
                }
            }

            // apply offset
            _offsetManager.AddCameraOffset(new Vector3(0f, _landingAnimPos, 0f));

            // keep track of player velocity
            _lastPlayerVel = _playerController.GetAttachedOWRigidbody().GetVelocity();
        }
        else
        {
            // reset landing anim parameters if feature is disabled
            _lastPlayerVel = Vector3.zero;
            _landingAnimPos = 0f;
            _landingAnimDampVel = 0f;
            _isLandingAnimActive = false;
        }
    }
}
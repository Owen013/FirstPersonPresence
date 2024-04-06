using UnityEngine;

namespace Immersion.Components;

public class AnimSpeedController : MonoBehaviour
{
    public static AnimSpeedController Instance { get; private set; }
    public float AnimSpeed {  get; private set; }

    private Animator _animator;
    private PlayerCharacterController _characterController;

    private void Awake()
    {
        Instance = this;
        _animator = GetComponent<Animator>();
        _characterController = Locator.GetPlayerController();
    }

    private void LateUpdate()
    {
        if (!_characterController.IsGrounded() && _characterController._fluidDetector.InFluidType(FluidVolume.Type.WATER))
        {
            AnimSpeed = 0.6f;
        }
        else
        {
            AnimSpeed = 1f;
        }

        // yield to hikers mod if installed, let it do the anim speed
        if (!ModMain.Instance.IsHikersModInstalled)
        {
            if (ModMain.Instance.SmolHatchlingAPI != null)
            {
                AnimSpeed *= ModMain.Instance.SmolHatchlingAPI.GetAnimSpeed();
            }
            _animator.speed = AnimSpeed;
        }
    }
}
using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

public class AnimSpeedController : MonoBehaviour
{
    public static AnimSpeedController Instance { get; private set; }

    public float AnimSpeed { get; private set; }

    private PlayerCharacterController _characterController;

    private Animator _animator;

    private void Awake()
    {
        Instance = this;
        _characterController = Locator.GetPlayerController();
        _animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (!_characterController.IsGrounded() && _characterController._fluidDetector.InFluidType(FluidVolume.Type.WATER))
            AnimSpeed = 0.6f;
        else
            AnimSpeed = 1f;

        // yield to hikers mod if installed, let it do the anim speed
        if (ModMain.Instance.HikersModAPI == null)
        {
            if (ModMain.Instance.SmolHatchlingAPI != null)
                AnimSpeed *= ModMain.Instance.SmolHatchlingAPI.GetPlayerAnimSpeed();
            _animator.speed = AnimSpeed;
        }
    }
}
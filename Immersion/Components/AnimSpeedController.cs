using HarmonyLib;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class AnimSpeedController : MonoBehaviour
{
    public static AnimSpeedController Instance { get; private set; }

    public float AnimSpeed {  get; private set; }

    private Animator _animator;

    private PlayerCharacterController _characterController;

    private GameObject[] _leftArmObjects;

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
        if (!ModMain.IsHikersModInstalled)
        {
            if (ModMain.SmolHatchlingAPI != null)
            {
                AnimSpeed *= ModMain.SmolHatchlingAPI.GetPlayerAnimSpeed();
            }
            _animator.speed = AnimSpeed;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.Start))]
    private static void AddToPlayerAnimator(PlayerAnimController __instance)
    {
        __instance.gameObject.AddComponent<AnimSpeedController>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
    private static void OnAnimControllerLateUpdate(PlayerAnimController __instance)
    {
        Instance._leftArmObjects ??=
        [
            __instance.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject,
            __instance.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject
        ];

        for (int i = 0; i < __instance._rightArmObjects.Length; i++)
        {
            __instance._rightArmObjects[i].layer = __instance._defaultLayer;
        }

        for (int i = 0; i < Instance._leftArmObjects.Length; i++)
        {
            Instance._leftArmObjects[i].layer = __instance._defaultLayer;
        }

        ToolMode toolMode = Locator.GetToolModeSwapper().GetToolMode();
        __instance._rightArmHidden = toolMode > ToolMode.None;
        if (ModMain.IsLeftyModeEnabled && toolMode != ToolMode.Translator)
        {
            for (int i = 0; i < __instance._rightArmObjects.Length; i++)
            {
                Instance._leftArmObjects[i].layer = __instance._rightArmHidden ? __instance._probeOnlyLayer : __instance._defaultLayer;
            }
        }
        else
        {
            for (int i = 0; i < __instance._rightArmObjects.Length; i++)
            {
                __instance._rightArmObjects[i].layer = __instance._rightArmHidden ? __instance._probeOnlyLayer : __instance._defaultLayer;
            }
        }
    }
}
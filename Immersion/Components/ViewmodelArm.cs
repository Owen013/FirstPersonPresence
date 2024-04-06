using UnityEngine;

namespace Immersion.Components;

public class ViewmodelArm : MonoBehaviour
{
    private bool _isItemToolArm;
    private GameObject _noSuitArm;
    private GameObject _suitArm;
    private GameObject _realNoSuitRightArm;
    private GameObject _realSuitRightArm;
    private GameObject _realNoSuitLeftArm;
    private GameObject _realSuitLeftArm;

    private void Start()
    {
        if (gameObject.GetComponentInParent<OWItem>() != null)
        {
            _isItemToolArm = true;
        }
        else
        {
            _isItemToolArm = false;
        }

        _noSuitArm = gameObject.transform.Find("Arm_NoSuit").gameObject;
        _suitArm = gameObject.transform.Find("Arm_Suit").gameObject;

        PlayerAnimController _playerVisuals = Locator.GetPlayerController().GetComponentInChildren<PlayerAnimController>();
        _realNoSuitRightArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _realSuitRightArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        _realNoSuitLeftArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
        _realSuitLeftArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;
    }

    private void Update()
    {
        if (!Config.IsViewModelHandsEnabled || (_isItemToolArm && !GetComponentInParent<ItemTool>()))
        {
            _noSuitArm.SetActive(false);
            _suitArm.SetActive(false);
        }
        else if (Config.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
        {
            _noSuitArm.SetActive(_realNoSuitLeftArm.activeInHierarchy);
            _suitArm.SetActive(_realSuitLeftArm.activeInHierarchy);
        }
        else
        {
            _noSuitArm.SetActive(_realNoSuitRightArm.activeInHierarchy);
            _suitArm.SetActive(_realSuitRightArm.activeInHierarchy);
        }
    }
}
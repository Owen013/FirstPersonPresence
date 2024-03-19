using UnityEngine;

namespace Immersion.Components;

public class ToolArm : MonoBehaviour
{
    protected GameObject _noSuitArm;
    protected GameObject _suitArm;
    protected GameObject _realNoSuitRightArm;
    protected GameObject _realSuitRightArm;
    protected GameObject _realNoSuitLeftArm;
    protected GameObject _realSuitLeftArm;

    protected virtual void Start()
    {
        _noSuitArm = gameObject.transform.Find("NoSuit").gameObject;
        _suitArm = gameObject.transform.Find("Suit").gameObject;
        PlayerAnimController _playerVisuals = Locator.GetPlayerController().GetComponentInChildren<PlayerAnimController>();
        _realNoSuitRightArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _realSuitRightArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        _realNoSuitLeftArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_LeftArm").gameObject;
        _realSuitLeftArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject;
    }

    protected virtual void Update()
    {
        if (!Config.IsViewModelHandsEnabled)
        {
            _noSuitArm.SetActive(false);
            _suitArm.SetActive(false);
            return;
        }

        if (Config.IsLeftyModeEnabled && Locator.GetToolModeSwapper()._currentToolMode != ToolMode.Translator)
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
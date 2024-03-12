using UnityEngine;

namespace Immersion.Components;

public class ToolArm : MonoBehaviour
{
    protected GameObject _noSuitArm;
    protected GameObject _suitArm;
    protected GameObject _realNoSuitArm;
    protected GameObject _realSuitArm;

    protected virtual void Start()
    {
        _noSuitArm = gameObject.transform.Find("NoSuit").gameObject;
        _suitArm = gameObject.transform.Find("Suit").gameObject;
        PlayerAnimController _playerVisuals = Locator.GetPlayerController().GetComponentInChildren<PlayerAnimController>();
        _realNoSuitArm = _playerVisuals.transform.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _realSuitArm = _playerVisuals.transform.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
    }

    protected virtual void Update()
    {
        if (!Config.UseViewmodelHands)
        {
            _noSuitArm.SetActive(false);
            _suitArm.SetActive(false);
        }

        _noSuitArm.SetActive(_realNoSuitArm.activeInHierarchy);
        _suitArm.SetActive(_realSuitArm.activeInHierarchy);
    }
}
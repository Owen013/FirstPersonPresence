using UnityEngine;

namespace Immersion.Components;

public class ToolArm : MonoBehaviour
{
    protected GameObject _noSuitArm;
    protected GameObject _suitArm;
    protected PlayerCharacterController _characterController;

    protected void Start()
    {
        _noSuitArm = gameObject.transform.Find("NoSuit").gameObject;
        _suitArm = gameObject.transform.Find("Suit").gameObject;
        _characterController = Locator.GetPlayerController();
    }

    protected void Update()
    {
        _noSuitArm.SetActive(!_characterController._isWearingSuit);
        _suitArm.SetActive(!_noSuitArm.activeSelf);
    }
}
namespace Immersion.Components;

public class ItemToolArm : ToolArm
{
    private void LateUpdate()
    {
        if (!GetComponentInParent<ItemTool>())
        {
            _noSuitArm.SetActive(false);
            _suitArm.SetActive(false);
        }
    }
}
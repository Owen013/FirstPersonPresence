using UnityEngine;

namespace Immersion.Components
{
    public class ItemToolArm : ToolArm
    {
        protected override void Update()
        {
            base.Update();
            if (!GetComponentInParent<ItemTool>())
            {
                _noSuitArm.SetActive(false);
                _suitArm.SetActive(false);
            }
        }
    }
}
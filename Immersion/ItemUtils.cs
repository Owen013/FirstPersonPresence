using OWML.Utils;
using UnityEngine;

namespace Immersion;

internal static class ItemUtils
{
    internal static bool IsBaseGameItem(OWItem item)
    {
        return item.GetItemType() switch
        {
            ItemType.SharedStone => item is SharedStone,
            ItemType.Scroll => item is ScrollItem,
            ItemType.ConversationStone => item is NomaiConversationStone,
            ItemType.WarpCore => item is WarpCoreItem,
            ItemType.Lantern => item is SimpleLanternItem,
            ItemType.SlideReel => item is SlideReelItem,
            ItemType.DreamLantern => item is DreamLanternItem,
            ItemType.VisionTorch => item is VisionTorchItem,
            _ => false
        };
    }

    internal static bool IsTSTAItem(OWItem item)
    {
        if (!ModMain.IsTheStrangerTheyAreInstalled) return false;

        string itemName = item.GetItemType().GetName();
        return
            itemName == "CloakMineral" ||
            itemName == "StrangerSeal" ||
            itemName == "GhostbirdSkull";
    }

    internal static void OnPickUpItem(OWItem item)
    {
        if (IsTSTAItem(item) && item.GetItemType().GetName() == "GhostbirdSkull")
        {
            // TSTA skull has weird bounds, so it can stop rendering when near the edges of the screen
            // normally isn't a problem, since its held position is not close enough to the edge of screen for this to be an issue
            // with Immersion installed, hand sway / hand height offset can cause skull to move far enough away to disappear
            // so set the renderers to update when "offscreen" while the skull is held
            foreach (var renderer in item.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = true;
        }
    }

    internal static void OnDropItem(OWItem item)
    {
        if (IsTSTAItem(item) && item.GetItemType().GetName() == "GhostbirdSkull")
        {
            foreach (var renderer in item.GetComponentsInChildren<SkinnedMeshRenderer>())
                renderer.updateWhenOffscreen = false;
        }
    }
}

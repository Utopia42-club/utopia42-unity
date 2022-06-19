using Source.AssetsInventory.Models;

namespace Source.AssetsInventory.slots
{
    public class AssetInventorySlot : BaseInventorySlot
    {
        private readonly bool updateImage;

        public AssetInventorySlot(bool updateImage = true)
        {
            this.updateImage = updateImage;
        }

        public override void SetSlotInfo(SlotInfo slotInfo)
        {
            base.SetSlotInfo(slotInfo);
            var asset = slotInfo.asset;
            if (asset == null)
                return;
            SetTooltip(asset.name);
            if (updateImage)
                LoadImage(asset.thumbnailUrl);
        }

        public override object Clone()
        {
            var clone = new AssetInventorySlot(IsLoadingImage());
            clone.SetSlotInfo(slotInfo);
            if (!IsLoadingImage())
                clone.SetBackground(GetBackground());
            return clone;
        }
    }
}
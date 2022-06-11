using src.AssetsInventory.Models;

namespace src.AssetsInventory.slots
{
    public class AssetInventorySlot : BaseInventorySlot
    {
        private Asset asset;

        public AssetInventorySlot(Asset asset, bool updateImage = true)
        {
            SetAsset(asset, updateImage);
        }

        public void SetAsset(Asset asset, bool updateImage = true)
        {
            this.asset = asset;
            if (asset == null) return;
            SetTooltip(asset.name);
            if (updateImage)
                LoadImage(asset.thumbnailUrl);
        }

        public Asset GetAsset()
        {
            return asset;
        }

        public override object Clone()
        {
            var clone = new AssetInventorySlot(asset, IsLoadingImage());
            if (!IsLoadingImage())
                clone.SetBackground(GetBackground());
            return clone;
        }
    }
}
using src.AssetsInventory.Models;
using src.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public class FavoriteItemInventorySlot : InventorySlotWrapper
    {
        public FavoriteItem favoriteItem;
        private VisualElement selectedBorder;

        public FavoriteItemInventorySlot(FavoriteItem favoriteItem)
        {
            SetFavoriteItem(favoriteItem);
            if (favoriteItem != null)
            {
                if (favoriteItem.asset != null)
                    UpdateSlot(new AssetInventorySlot(favoriteItem.asset));
                else if (favoriteItem.blockId.HasValue)
                {
                    if (ColorBlocks.IsColorTypeId(favoriteItem.blockId.Value, out var blockType))
                        UpdateSlot(new ColorBlockInventorySlot(blockType.name));
                    else
                        UpdateSlot(new BlockInventorySlot(Blocks.GetBlockType(favoriteItem.blockId.Value)));
                }
            }
            else
                UpdateSlot(new SimpleInventorySlot());
        }

        public void SetFavoriteItem(FavoriteItem favoriteItem)
        {
            this.favoriteItem = favoriteItem;
        }

        protected override void InitSlot()
        {
            base.InitSlot();

            if (favoriteItem == null)
                return;

            currentSlot.ConfigLeftAction("Delete", Resources.Load<Sprite>("Icons/close"),
                () => currentSlot.assetsInventory.DeleteFavoriteItem(this));

            currentSlot.slot.RegisterCallback<MouseEnterEvent>(evt => currentSlot.SetLeftActionVisible(true));
            currentSlot.slot.RegisterCallback<MouseLeaveEvent>(evt => currentSlot.SetLeftActionVisible(false));

            selectedBorder = currentSlot.slot.Q<VisualElement>("selectedBorder");
            currentSlot.slot.RegisterCallback<PointerDownEvent>(evt =>
                currentSlot.assetsInventory.SelectFavoriteItem(this));
        }


        public void SetSelected(bool selected)
        {
            if (selectedBorder != null)
                selectedBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetSize(int size, int margin = 0)
        {
            base.SetSize(size, margin);
            GetCurrentSlot()?.SetSize(size, margin);
        }
    }
}
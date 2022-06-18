using src.AssetsInventory.Models;
using src.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public class FavoriteItemInventorySlot : InventorySlotWrapper
    {
        public readonly FavoriteItem favoriteItem;

        public FavoriteItemInventorySlot(FavoriteItem favoriteItem, int size = 80)
        {
            this.favoriteItem = favoriteItem;
            SetSize(size);
            if (favoriteItem != null)
            {
                if (favoriteItem.asset != null)
                    SetSlotInfo(new SlotInfo(favoriteItem.asset));
                else if (favoriteItem.blockId.HasValue)
                    SetSlotInfo(new SlotInfo(Blocks.GetBlockType(favoriteItem.blockId.Value)));
            }
            else
                SetSlotInfo(null);
        }

        protected override void InitSlot()
        {
            base.InitSlot();

            if (favoriteItem == null)
                return;

            currentSlot.ConfigLeftAction("Delete", Resources.Load<Sprite>("Icons/close"),
                () => currentSlot.assetsInventory.RemoveFromFavorites(favoriteItem, currentSlot, () =>
                {
                    AssetsInventory.INSTANCE.ReloadTab();
                }));
            currentSlot.slot.RegisterCallback<MouseEnterEvent>(evt => currentSlot.SetLeftActionVisible(true));
            currentSlot.slot.RegisterCallback<MouseLeaveEvent>(evt => currentSlot.SetLeftActionVisible(false));
        }
    }
}
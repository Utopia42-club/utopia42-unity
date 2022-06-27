using System;
using Source.Ui.AssetInventory.Models;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Slots
{
    public interface InventorySlot : ICloneable, UiProvider
    {
        public void SetSize(int size, int iconMargin);
        public void SetSlotInfo(SlotInfo slotInfo);
        public SlotInfo GetSlotInfo();
        public void SetSelected(bool selected);
        public void SetSelectable(bool selectable);
    }
}
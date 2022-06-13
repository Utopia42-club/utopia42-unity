using System;
using src.AssetsInventory.Models;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public interface InventorySlot : ICloneable
    {
        public VisualElement VisualElement();
        public void SetSize(int size, int iconMargin);
        public void SetSlotInfo(SlotInfo slotInfo);
        public SlotInfo GetSlotInfo();
        public void SetSelected(bool selected);
        public void SetSelectable(bool selectable);
    }
}
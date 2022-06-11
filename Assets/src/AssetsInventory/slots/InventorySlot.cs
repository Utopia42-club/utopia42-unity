using System;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public interface InventorySlot : ICloneable
    {
        public VisualElement VisualElement();
        public void SetSize(int size, int iconMargin);
    }
}
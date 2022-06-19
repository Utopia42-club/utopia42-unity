using UnityEngine;
using UnityEngine.UIElements;

namespace Source.AssetsInventory.slots
{
    public class HandyItemInventorySlot : InventorySlotWrapper
    {
        public HandyItemInventorySlot()
        {
        }

        protected override void InitSlot()
        {
            base.InitSlot();
            currentSlot.ConfigLeftAction("Delete", Resources.Load<Sprite>("Icons/close"),
                () => currentSlot.assetsInventory.RemoveFromHandyPanel(this));
            currentSlot.slot.RegisterCallback<MouseEnterEvent>(evt => currentSlot.SetLeftActionVisible(true));
            currentSlot.slot.RegisterCallback<MouseLeaveEvent>(evt => currentSlot.SetLeftActionVisible(false));
            currentSlot.SetOnSelect(() => currentSlot.assetsInventory.SelectSlot(this, false));
        }
    }
}
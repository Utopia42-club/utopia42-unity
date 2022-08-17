using Source.Model.Inventory;

namespace Source.Ui.AssetInventory.Slots
{
    public class BlockInventorySlot : BaseInventorySlot
    {
        public override void SetSlotInfo(SlotInfo slotInfo)
        {
            base.SetSlotInfo(slotInfo);
            var block = slotInfo.block;
            if (block == null) return;
            SetTooltip(block.name);
            SetBackground(block.GetIcon(), false);
        }

        public override object Clone()
        {
            var clone = new BlockInventorySlot();
            clone.SetSlotInfo(slotInfo);
            return clone;
        }
    }
}
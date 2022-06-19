using Source.AssetsInventory.Models;

namespace Source.AssetsInventory.slots
{
    public class BlockInventorySlot : BaseInventorySlot
    {
        public override void SetSlotInfo(SlotInfo slotInfo)
        {
            base.SetSlotInfo(slotInfo);
            var block = slotInfo.block;
            if (block == null) return;
            SetTooltip(block.name);
            SetBackground(block.GetIcon());
        }

        public override object Clone()
        {
            var clone = new BlockInventorySlot();
            clone.SetSlotInfo(slotInfo);
            return clone;
        }
    }
}
using src.Model;

namespace src.AssetsInventory.slots
{
    public class BlockInventorySlot : BaseInventorySlot
    {
        private BlockType block;

        public BlockInventorySlot(BlockType block)
        {
            SetBlock(block);
        }

        public void SetBlock(BlockType block)
        {
            this.block = block;
            if (block == null) return;
            SetTooltip(block.name);
            SetBackground(block.GetIcon());
        }

        public BlockType GetBlock()
        {
            return block;
        }

        public override object Clone()
        {
            var slot = new BlockInventorySlot(block);
            return slot;
        }
    }
}
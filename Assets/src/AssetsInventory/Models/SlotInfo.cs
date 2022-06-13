using src.Model;

namespace src.AssetsInventory.Models
{
    public class SlotInfo
    {
        public Asset asset { get; set; }
        public BlockType block { get; set; }

        public SlotInfo(Asset asset)
        {
            this.asset = asset;
        }

        public SlotInfo(BlockType block)
        {
            this.block = block;
        }

        public SlotInfo()
        {
        }
    }
}
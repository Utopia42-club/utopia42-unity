namespace Source.Model.Inventory
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

        public bool IsEmpty()
        {
            return asset == null && block == null;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj is SlotInfo slotInfo)
                return (slotInfo.asset != null && asset != null && slotInfo.asset.id.Value == asset.id.Value)
                       || (slotInfo.block != null && block != null && slotInfo.block.id == block.id);
            return false;
        }
    }
}
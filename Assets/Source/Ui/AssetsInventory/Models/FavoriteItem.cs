using Source.Utils;

namespace Source.AssetsInventory.Models
{
    public class FavoriteItem
    {
        public int? id { get; set; }
        public string walletId { get; set; }
        public Asset asset { get; set; }
        public uint? blockId { get; set; }

        public SlotInfo ToSlotInfo()
        {
            var s = new SlotInfo
            {
                asset = asset
            };
            if (blockId.HasValue && blockId.Value != 0)
                s.block = Blocks.GetBlockType(blockId.Value);
            return s;
        }
    }
}
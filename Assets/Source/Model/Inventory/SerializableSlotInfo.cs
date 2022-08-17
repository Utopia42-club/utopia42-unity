using Source.Utils;

namespace Source.Model.Inventory
{
    public class SerializableSlotInfo
    {
        public Asset asset { get; set; }
        public uint blockId { get; set; }

        public static SerializableSlotInfo FromSlotInfo(SlotInfo slotInfo)
        {
            var s = new SerializableSlotInfo
            {
                asset = slotInfo.asset
            };
            if (slotInfo.block != null)
                s.blockId = slotInfo.block.id;
            return s;
        }

        public SlotInfo ToSlotInfo()
        {
            var s = new SlotInfo
            {
                asset = asset
            };
            if (blockId != 0)
                s.block = Blocks.GetBlockType(blockId);
            return s;
        }
    }
}
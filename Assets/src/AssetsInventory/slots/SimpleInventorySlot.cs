namespace src.AssetsInventory.slots
{
    public class SimpleInventorySlot : BaseInventorySlot
    {
        public SimpleInventorySlot()
        {
        }

        public override object Clone()
        {
            return new SimpleInventorySlot();
        }
    }
}
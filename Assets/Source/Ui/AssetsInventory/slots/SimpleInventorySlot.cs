namespace Source.Ui.AssetsInventory.slots
{
    public class SimpleInventorySlot : BaseInventorySlot
    {
        public override object Clone()
        {
            return new SimpleInventorySlot();
        }
    }
}
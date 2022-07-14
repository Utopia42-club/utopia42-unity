namespace Source.Ui.AssetInventory.Slots
{
    public class SimpleInventorySlot : BaseInventorySlot
    {
        public override object Clone()
        {
            return new SimpleInventorySlot();
        }
    }
}
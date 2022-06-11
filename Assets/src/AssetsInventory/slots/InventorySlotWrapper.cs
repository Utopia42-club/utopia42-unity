using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public class InventorySlotWrapper : InventorySlot
    {
        protected BaseInventorySlot currentSlot;

        private readonly VisualElement root;

        private readonly bool hideTooltip;
        private int size;

        public InventorySlotWrapper(bool hideTooltip = false)
        {
            this.hideTooltip = hideTooltip;
            root = new VisualElement();
        }

        public void SetSize(int size, int iconMargin = 0)
        {
            this.size = size;
            root.style.width = size;
            root.style.height = size;
        }

        public void UpdateSlot(InventorySlot sourceSlot)
        {
            currentSlot = (BaseInventorySlot) sourceSlot.Clone();
            currentSlot.SetSize(size);
            if (hideTooltip)
                currentSlot.SetTooltip(null);
            root.Clear();
            root.Add(currentSlot.VisualElement());
            InitSlot();
        }

        protected virtual void InitSlot()
        {
        }

        public VisualElement VisualElement()
        {
            return root;
        }

        public BaseInventorySlot GetCurrentSlot()
        {
            return currentSlot;
        }

        public object Clone()
        {
            return currentSlot.Clone();
        }
    }
}
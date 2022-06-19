using Source.Ui.AssetsInventory.Models;
using UnityEngine.UIElements;

namespace Source.Ui.AssetsInventory.slots
{
    public class InventorySlotWrapper : InventorySlot
    {
        protected BaseInventorySlot currentSlot;

        private readonly VisualElement root;

        private readonly bool hideTooltip;
        private int size;
        private int iconMargin;

        public InventorySlotWrapper(bool hideTooltip = false)
        {
            this.hideTooltip = hideTooltip;
            root = new VisualElement();
        }

        public void SetSize(int size, int iconMargin = 0)
        {
            this.size = size;
            this.iconMargin = iconMargin;
            root.style.width = size;
            root.style.height = size;
        }

        public void SetSlotInfo(SlotInfo slotInfo)
        {
            if (slotInfo == null)
            {
                var simpleInventorySlot = new SimpleInventorySlot();
                simpleInventorySlot.SetSlotInfo(slotInfo);
                UpdateSlot(simpleInventorySlot);
            }
            else if (slotInfo.asset != null)
            {
                var assetInventorySlot = new AssetInventorySlot();
                assetInventorySlot.SetSlotInfo(slotInfo);
                UpdateSlot(assetInventorySlot);
            }
            else if (slotInfo.block != null)
            {
                var blockInventorySlot = slotInfo.block.IsColorBlockType()
                    ? new ColorBlockInventorySlot()
                    : new BlockInventorySlot();
                blockInventorySlot.SetSlotInfo(slotInfo);
                UpdateSlot(blockInventorySlot);
            }
        }

        public SlotInfo GetSlotInfo()
        {
            return currentSlot.GetSlotInfo();
        }

        public void UpdateSlot(InventorySlot sourceSlot)
        {
            currentSlot = (BaseInventorySlot) sourceSlot.Clone();
            currentSlot.SetSize(size, iconMargin);
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

        public void SetSelected(bool selected)
        {
            GetCurrentSlot().SetSelected(selected);
        }

        public void SetSelectable(bool selectable)
        {
            GetCurrentSlot().SetSelectable(selectable);
        }

        public object Clone()
        {
            return currentSlot.Clone();
        }
    }
}
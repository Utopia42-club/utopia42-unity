using System;
using Source.AssetsInventory.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.AssetsInventory.slots
{
    public class ColorBlockInventorySlot : BlockInventorySlot
    {
        public Color color;

        public ColorBlockInventorySlot(bool addDeleteAction = true)
        {
            if (addDeleteAction)
            {
                ConfigLeftAction("Delete", Resources.Load<Sprite>("Icons/close"),
                    () => assetsInventory.DeleteColorBlock(this));
                slot.RegisterCallback<MouseEnterEvent>(evt => SetLeftActionVisible(true));
                slot.RegisterCallback<MouseLeaveEvent>(evt => SetLeftActionVisible(false));
            }
        }

        public override void SetSlotInfo(SlotInfo slotInfo)
        {
            base.SetSlotInfo(slotInfo);
            if (slotInfo.block.color == null)
                throw new Exception("Invalid SlotInfo for color slot");
            color = slotInfo.block.color.Value;
            SetBackgroundColor(color);
        }

        private void SetBackgroundColor(Color col)
        {
            slotIcon.style.unityBackgroundImageTintColor = col;
        }

        public override object Clone()
        {
            var clone = new ColorBlockInventorySlot(false);
            clone.SetSlotInfo(slotInfo);
            return clone;
        }
    }
}
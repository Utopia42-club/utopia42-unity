using System;
using Source.Ui.AssetInventory.Models;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Slots
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
                RegisterCallback<MouseEnterEvent>(evt => SetLeftActionVisible(true));
                RegisterCallback<MouseLeaveEvent>(evt => SetLeftActionVisible(false));
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
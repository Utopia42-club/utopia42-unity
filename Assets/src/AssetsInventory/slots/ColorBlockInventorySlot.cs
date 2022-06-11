using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public class ColorBlockInventorySlot : BlockInventorySlot
    {
        public readonly Color color;
        private readonly string colorString;

        public ColorBlockInventorySlot(string color, bool addDeleteAction = true) : base(null)
        {
            colorString = color;
            this.color = Colors.ConvertHexToColor(color) ?? Color.white;
            SetBlock(ColorBlocks.GetBlockTypeFromColor(this.color));

            if (addDeleteAction)
            {
                ConfigLeftAction("Delete", Resources.Load<Sprite>("Icons/close"),
                    () => assetsInventory.DeleteColorBlock(this));
                slot.RegisterCallback<MouseEnterEvent>(evt => SetLeftActionVisible(true));
                slot.RegisterCallback<MouseLeaveEvent>(evt => SetLeftActionVisible(false));
            }
        }

        public void SetBlock(BlockType block)
        {
            base.SetBlock(block);
            SetBackgroundColor(color);
        }

        private void SetBackgroundColor(Color col)
        {
            slotIcon.style.unityBackgroundImageTintColor = col;
        }

        public override object Clone()
        {
            return new ColorBlockInventorySlot(colorString, false);
        }
    }
}
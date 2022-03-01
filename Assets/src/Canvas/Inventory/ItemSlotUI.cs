using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class ItemSlotUI : MonoBehaviour
    {
        public ItemSlot itemSlot;
        public Image slotIcon;
        public Text slotAmount;

        public void SetItemSlot(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;

            if (itemSlot != null && itemSlot.GetStack() != null)
            {
                var blockId = itemSlot.GetStack().id;
                slotIcon.sprite = WorldService.INSTANCE.GetBlockType(blockId).GetIcon();

                if (ColorBlocks.IsColorTypeId(blockId, out var colorType))
                {
                    slotIcon.color = ColorBlocks.GetColorFromBlockType(colorType);
                }

                slotAmount.text = "";
                slotAmount.enabled = true;
                slotIcon.enabled = true;
            }
            else
            {
                slotIcon.sprite = null;
                slotAmount.text = "";
                slotAmount.enabled = false;
                slotIcon.enabled = false;
            }
        }

        public ItemSlot GetItemSlot()
        {
            return itemSlot;
        }

        public bool HasItem()
        {
            return itemSlot != null && itemSlot.GetStack() != null;
        }

        public void UpdateView()
        {
            SetItemSlot(itemSlot);
        }
    }
}
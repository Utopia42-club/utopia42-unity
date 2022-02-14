using src.Service;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class ItemSlotUI : MonoBehaviour
    {
        public ItemSlot itemSlot;
        public Image slotImage;
        public Image slotIcon;
        public Text slotAmount;

        public void SetItemSlot(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;

            if (itemSlot != null && itemSlot.GetStack() != null)
            {
                slotIcon.sprite = VoxelService.INSTANCE.GetBlockType(itemSlot.GetStack().id).GetIcon();
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
            SetItemSlot(this.itemSlot);
        }


    }
}

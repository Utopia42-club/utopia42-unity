using src.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas
{
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ItemSlot itemSlot;
        public Image slotIcon;
        public Text slotAmount;
        public Button deleteButton;
        private bool deleteEnabled;

        public void SetItemSlot(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;

            if (itemSlot?.GetStack() != null)
            {
                var blockId = itemSlot.GetStack().id;
                slotIcon.sprite = Blocks.GetBlockType(blockId).GetIcon();

                if (ColorBlocks.IsColorTypeId(blockId, out var colorType))
                {
                    slotIcon.color = ColorBlocks.GetColorFromBlockType(colorType);
                    deleteEnabled = itemSlot.GetFromInventory();
                }
                else
                {
                    deleteEnabled = false;
                    slotIcon.color = Color.white;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            deleteButton.gameObject.SetActive(deleteEnabled);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }
}
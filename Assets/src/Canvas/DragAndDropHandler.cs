using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas
{
    public class DragAndDropHandler : MonoBehaviour
    {
        [SerializeField] public ItemSlotUI cursorSlot = null;
        private ItemSlot cursorItemSlot;

        [SerializeField]
        private GraphicRaycaster raycaster = null;
        private PointerEventData pointerEventData;
        [SerializeField]
        private EventSystem eventSystem = null;

        World world;

        private void Start()
        {
            cursorItemSlot = new ItemSlot();
            cursorItemSlot.SetUi(cursorSlot);
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.INVENTORY) return;

            cursorSlot.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                HandleSlotClick(CheckForSlot());
            }
        }

        private void HandleSlotClick(ItemSlotUI clickedSlot)
        {
            if (clickedSlot == null)
                return;

            if (!cursorSlot.HasItem() && !clickedSlot.HasItem())
                return;

            if (clickedSlot.itemSlot.GetFromInventory())
            {
                cursorItemSlot.SetStack(clickedSlot.itemSlot.GetStack());
                return;
            }

            if (!cursorSlot.HasItem() && clickedSlot.HasItem())
            {
                cursorItemSlot.SetStack(clickedSlot.itemSlot.HandOverStack());
                return;
            }

            if (cursorSlot.HasItem() && !clickedSlot.HasItem())
            {
                clickedSlot.itemSlot.SetStack(cursorItemSlot.HandOverStack());
                return;
            }

            if (cursorSlot.HasItem() && clickedSlot.HasItem())
            {
                if (cursorSlot.itemSlot.GetStack().id != clickedSlot.itemSlot.GetStack().id)
                {
                    ItemStack oldCursorSlot = cursorSlot.itemSlot.HandOverStack();
                    ItemStack oldSlot = clickedSlot.itemSlot.HandOverStack();

                    clickedSlot.itemSlot.SetStack(oldCursorSlot);
                    // cursorSlot.itemSlot.SetStack(oldSlot);
                }
            }
        }

        private ItemSlotUI CheckForSlot()
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.tag == "ItemSlotUI")
                    return result.gameObject.GetComponent<ItemSlotUI>();
            }

            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using src.Utils;
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

        public Inventory inventory;
        
        World world;

        public bool enabled = true;

        private void Start()
        {
            cursorItemSlot = new ItemSlot();
            cursorItemSlot.SetUi(cursorSlot);
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.INVENTORY || !enabled) return;

            cursorSlot.transform.position = Input.mousePosition;
    
            if (Input.GetMouseButtonDown(1))
            {
                ClearCursorSlot();
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                                                      || Input.GetKey(KeyCode.LeftCommand)
                                                      || Input.GetKey(KeyCode.RightCommand))
                {
                    RequestTypeDelete(CheckForSlot());
                }
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                HandleSlotClick(CheckForSlot());
            }
        }

        private void RequestTypeDelete(ItemSlotUI clickedSlot)
        {
            if (!cursorSlot.HasItem() && !clickedSlot.HasItem())
                return;

            if (clickedSlot.itemSlot.GetFromInventory())
            {
                var id = clickedSlot.itemSlot.GetStack().id;
                if (ColorBlocks.IsColorTypeId(id))
                {
                    StartCoroutine(inventory.RemoveColorBlock(clickedSlot));
                }
            }
        }

        private void ClearCursorSlot()
        {
            cursorItemSlot.SetStack(null);
        }

        private void HandleSlotClick(ItemSlotUI clickedSlot)
        {
            if (clickedSlot == null)
            {
                ClearCursorSlot();
                return;
            }

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
                    var oldCursorSlot = cursorSlot.itemSlot.HandOverStack();
                    var oldSlot = clickedSlot.itemSlot.HandOverStack();

                    clickedSlot.itemSlot.SetStack(oldCursorSlot);
                    // cursorSlot.itemSlot.SetStack(oldSlot);
                }
            }
        }

        private ItemSlotUI CheckForSlot()
        {
            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            return (from result in results where result.gameObject.CompareTag("ItemSlotUI") select result.gameObject.GetComponent<ItemSlotUI>()).FirstOrDefault();
        }
    }
}

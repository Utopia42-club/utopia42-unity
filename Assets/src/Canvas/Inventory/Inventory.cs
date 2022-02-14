using src.Service;
using UnityEngine;

namespace src.Canvas
{
    public class Inventory : MonoBehaviour
    {
        public GameObject slotPrefab;

        public ItemSlotUI cursorSlot;

        public ActionButton closeButton;

        private void Start()
        {
            var manager = GameManager.INSTANCE;

            for (var i = 1; i < UtopiaService.INSTANCE.GetBlockTypesCount(); i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, transform);

                ItemStack stack = new ItemStack((byte)i, 64);
                ItemSlot slot = new ItemSlot();
                slot.SetStack(stack);
                slot.SetUi(newSlot.GetComponent<ItemSlotUI>());
                slot.SetFromInventory(true);
            }

            manager.stateChange.AddListener(state =>
            {
                if (state != GameManager.State.INVENTORY)
                    cursorSlot.GetItemSlot().SetStack(null);
            });

            closeButton.AddListener(() =>
            {
                if (manager.GetState() == GameManager.State.INVENTORY)
                    manager.ReturnToGame();
            });
        }
    }
}

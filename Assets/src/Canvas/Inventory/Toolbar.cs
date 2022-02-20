using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class Toolbar : MonoBehaviour
    {
        private int selectedSlot = 0;

        public Player player;
        public RectTransform highlight;

        public GameObject slotPrefab;

        private ItemSlotUI[] slots = new ItemSlotUI[9];

        private void Start()
        {
            HorizontalLayoutGroup layout = GetComponentInChildren<HorizontalLayoutGroup>();
            for (int i = 1; i < 10; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, layout.transform);

                ItemStack stack = new ItemStack((byte)i, Random.Range(2, 65));
                ItemSlot slot = new ItemSlot();
                slot.SetStack(stack);
                var ui = newSlot.GetComponent<ItemSlotUI>();
                slot.SetUi(ui);
                slot.SetFromInventory(false);
                slots[i - 1] = ui;
            }

            SelectedChanged();
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            var dec = Input.GetButtonDown("Select Left");
            var inc = Input.GetButtonDown("Select Right");
            if (dec || inc)
            {
                if (dec) selectedSlot--;
                if (inc) selectedSlot++;
                selectedSlot = (selectedSlot + slots.Length) % slots.Length;
            }
            SelectedChanged();
        }

        private void SelectedChanged()
        {
            highlight.position = slots[selectedSlot].transform.position;
            if (slots[selectedSlot].GetItemSlot().GetStack() != null)
                player.selectedBlockId = slots[selectedSlot].GetItemSlot().GetStack().id;
            else player.selectedBlockId = 0;
        }

    }
}
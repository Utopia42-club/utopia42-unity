using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class Toolbar : MonoBehaviour
    {
        public Player player;
        public RectTransform highlight;
        public RectTransform hammerHighlight;

        public GameObject slotPrefab;

        private ItemSlotUI[] slots = new ItemSlotUI[9];
        private int selectedSlot = 0;

        private void Start()
        {
            HorizontalLayoutGroup layout = GetComponentInChildren<HorizontalLayoutGroup>();
            for (int i = 1; i < 10; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, layout.transform);

                ItemStack stack = new ItemStack((byte) i, Random.Range(2, 65));
                ItemSlot slot = new ItemSlot();
                slot.SetStack(stack);
                var ui = newSlot.GetComponent<ItemSlotUI>();
                slot.SetFromInventory(false);
                slot.SetUi(ui);
                slots[i - 1] = ui;
            }

            SelectedChanged();
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING) SelectedChanged();
            });
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;

            var mouseDelta = Input.mouseScrollDelta.y;
            var dec = (Input.GetButtonDown("Change Block") &&
                       (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) || mouseDelta <= -0.1;
            var inc = !dec && (Input.GetButtonDown("Change Block") || mouseDelta >= 0.1);
            if (!dec && !inc) return;
            if (dec) selectedSlot--;
            if (inc) selectedSlot++;
            selectedSlot = (selectedSlot + slots.Length + 1) % (slots.Length + 1);
            SelectedChanged();
        }

        private void SelectedChanged()
        {
            var hammerSelected = selectedSlot == slots.Length;
            highlight.gameObject.SetActive(!hammerSelected);
            hammerHighlight.gameObject.SetActive(hammerSelected);
            if (!hammerSelected)
            {
                highlight.position = slots[selectedSlot].transform.position;
                if (slots[selectedSlot].GetItemSlot().GetStack() != null)
                    player.selectedBlockId = slots[selectedSlot].GetItemSlot().GetStack().id;
                else player.selectedBlockId = 0;
            }

            player.ToolbarSelectedChanged(hammerSelected);
        }

        public static Toolbar INSTANCE => GameObject.Find("Toolbar").GetComponent<Toolbar>();
    }
}
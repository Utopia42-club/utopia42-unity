using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace src.Canvas
{
    public class Toolbar : MonoBehaviour
    {
        public Player player;
        public RectTransform highlight;
        public RectTransform hammerHighlight;
        public Button inventoryButton;

        public GameObject slotPrefab;

        private ItemSlotUI[] slots = new ItemSlotUI[9];
        private int selectedSlot = 0;

        private void Start()
        {
            inventoryButton.onClick.AddListener(() => GameManager.INSTANCE.OpenInventory());

            var layout = GetComponentInChildren<HorizontalLayoutGroup>();
            for (var i = 1; i < 10; i++)
            {
                var newSlot = Instantiate(slotPrefab, layout.transform);

                var stack = new ItemStack((byte) i, Random.Range(2, 65));
                var slot = new ItemSlot();
                slot.SetStack(stack);
                var ui = newSlot.GetComponent<ItemSlotUI>();
                slot.SetFromInventory(false);
                slot.SetUi(ui);
                slots[i - 1] = ui;
            }

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING && !player.ChangeForbidden) SelectedChanged();
            });
            SelectedChanged();
            if (player.ChangeForbidden)
            {
                highlight.gameObject.SetActive(false);
                hammerHighlight.gameObject.SetActive(false);
            }

            player.viewModeChanged.AddListener(vm =>
            {
                if (!player.ChangeForbidden)
                {
                    SelectedChanged();
                }
                else if (vm == Player.ViewMode.THIRD_PERSON)
                {
                    highlight.gameObject.SetActive(false);
                    hammerHighlight.gameObject.SetActive(false);
                }
            });
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING || player.ChangeForbidden) return;

            var mouseDelta = Input.mouseScrollDelta.y;
            var dec = Input.GetButtonDown("Change Block")
                      && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                      || mouseDelta <= -0.1;
            var inc = !dec && (Input.GetButtonDown("Change Block") || mouseDelta >= 0.1);
            if (!dec && !inc)
            {
                // SelectedChanged();
                return;
            }

            if (dec) selectedSlot--;
            if (inc) selectedSlot++;
            selectedSlot = (selectedSlot + slots.Length + 1) % (slots.Length + 1);
            SelectedChanged();
        }

        private void SelectedChanged()
        {
            // var hammerSelected = selectedSlot == slots.Length;
            // highlight.gameObject.SetActive(!hammerSelected);
            // hammerHighlight.gameObject.SetActive(hammerSelected);
            // if (!hammerSelected)
            // {
            //     highlight.position = slots[selectedSlot].transform.position;
            //     if (slots[selectedSlot].GetItemSlot().GetStack() != null)
            //         player.selectedBlockId = slots[selectedSlot].GetItemSlot().GetStack().id;
            //     else player.selectedBlockId = 0;
            // }
            //
            // player.ToolbarSelectedChanged(hammerSelected);
        }

        public static Toolbar INSTANCE => GameObject.Find("Toolbar")?.GetComponent<Toolbar>();
    }
}
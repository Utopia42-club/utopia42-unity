using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src.Canvas
{
    public class Inventory : MonoBehaviour
    {
        public static readonly string PLAYER_COLOR_BLOCKS = "PLAYER_COLOR_BLOCKS";

        public GameObject slotPrefab;
        public GameObject colorSlotPrefab;
        public ItemSlotUI cursorSlot;
        public ActionButton closeButton;
        private GameObject colorSlot;
        private ColorItemSlot colorItemSlot;

        private void Start()
        {
            var manager = GameManager.INSTANCE;

            foreach (var blockType in Blocks.GetBlockTypes())
            {
                if (blockType.name == "air")
                    continue;
                CreateSlot(blockType);
            }

            colorSlot = Instantiate(colorSlotPrefab, transform);

            colorItemSlot = colorSlot.GetComponent<ColorItemSlot>();
            colorItemSlot.onColorCreated = color => { AddColorBlock(color); };

            foreach (var colorBlock in GetPlayerColorBlocks())
            {
                var color = Colors.ConvertHexToColor(colorBlock);
                if (color.HasValue)
                    AddColorBlock(color.Value);
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

        private static void SaveBlockColor(Color color)
        {
            var colorBlocks = GetPlayerColorBlocks();
            colorBlocks.Add("#" + ColorUtility.ToHtmlStringRGB(color));
            SetPlayerColorBlocks(colorBlocks);
        }

        private static void RemoveBlockColorFromSaving(Color color)
        {
            var colorBlocks = GetPlayerColorBlocks();
            colorBlocks = colorBlocks.Where(c => c != "#" + ColorUtility.ToHtmlStringRGB(color)).ToList();
            SetPlayerColorBlocks(colorBlocks);
        }

        private void AddColorBlock(Color color)
        {
            CreateSlot(ColorBlocks.GetBlockTypeFromColor(color));
            UpdateColorSlotPosition();
            SaveBlockColor(color);
        }

        public IEnumerator RemoveColorBlock(ItemSlotUI itemSlotUI)
        {
            Destroy(itemSlotUI.gameObject);
            yield return null;
            UpdateColorSlotPosition(); // Should have get called on the next frame
            RemoveBlockColorFromSaving(itemSlotUI.slotIcon.color);
        }

        private void UpdateColorSlotPosition()
        {
            colorSlot.transform.SetAsLastSibling();
            var mod = colorSlot.transform.parent.childCount % 9;
            colorItemSlot.SetPanelSide(
                mod == 5 ? ColorItemSlot.ColorPanelSide.Middle :
                mod == 0 ? ColorItemSlot.ColorPanelSide.Left :
                mod < 5 ? ColorItemSlot.ColorPanelSide.Right : ColorItemSlot.ColorPanelSide.Left);
        }

        private static void SetPlayerColorBlocks(IEnumerable colorBlocks)
        {
            PlayerPrefs.SetString(PLAYER_COLOR_BLOCKS, JsonConvert.SerializeObject(colorBlocks));
        }

        private static List<string> GetPlayerColorBlocks()
        {
            var deserializeObject =
                JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(PLAYER_COLOR_BLOCKS, "[]"));
            return new HashSet<string>(deserializeObject).ToList();
        }

        private void CreateSlot(BlockType blockType)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);
            ItemStack stack = new ItemStack(blockType.id, 64);
            ItemSlot slot = new ItemSlot();
            slot.SetStack(stack);
            slot.SetFromInventory(true);
            slot.SetUi(newSlot.GetComponent<ItemSlotUI>());
        }
    }
}
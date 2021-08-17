using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject slotPrefab;   

    public ItemSlotUI cursorSlot;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        for (int i = 1; i < VoxelService.INSTANCE.GetBlockTypesCount(); i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);

            ItemStack stack = new ItemStack((byte)i, 64);
            ItemSlot slot = new ItemSlot();
            slot.SetStack(stack);
            slot.SetUi(newSlot.GetComponent<ItemSlotUI>());
            slot.SetFromInventory(true);
        }

        GameManager.INSTANCE.stateChange.AddListener(state =>
        {
            if (state != GameManager.State.INVENTORY)
                cursorSlot.GetItemSlot().SetStack(null);
        });
    }
}
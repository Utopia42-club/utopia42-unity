using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemSlotUI : MonoBehaviour
{
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;

    Blocks blocks;

    private void Awake()
    {
        blocks = GameObject.Find("Blocks").GetComponent<Blocks>();
    }


    public void SetItemSlot(ItemSlot itemSlot)
    {
        this.itemSlot = itemSlot;

        if (itemSlot != null && itemSlot.GetStack() != null)
        {
            slotIcon.sprite = blocks.blockIcons[itemSlot.GetStack().id];
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

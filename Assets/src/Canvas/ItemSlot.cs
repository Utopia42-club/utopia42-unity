using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot
{
    private ItemStack stack = null;
    private ItemSlotUI ui = null;
    private bool fromInventory = false;

    public void SetStack(ItemStack stack)
    {
        this.stack = stack;
        if (ui)
            ui.UpdateView();
    }

    public ItemStack GetStack()
    {
        return stack;
    }


    public ItemStack HandOverStack()
    {
        var st = stack;
        this.stack = null;
        if (ui)
            ui.UpdateView();
        return st;
    }

    public void SetUi(ItemSlotUI ui)
    {
        this.ui = ui;
        ui.SetItemSlot(this);
    }

    public ItemSlotUI GetUI()
    {
        return this.ui;
    }

    public void SetFromInventory(bool b)
    {
        fromInventory = b;
    }

    public bool GetFromInventory()
    {
        return fromInventory;
    }

    public int Take(int amt)
    {
        if (amt >= stack.amount)
        {
            int _amt = stack.amount;
            Clear();
            return _amt;
        }
        else
        {
            stack.amount -= amt;
            ui.UpdateView();
            return amt;
        }
    }

    private void Clear()
    {
        this.ui.SetItemSlot(null);
    }
}

public class ItemStack
{
    public int id;
    public int amount;

    public ItemStack(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}
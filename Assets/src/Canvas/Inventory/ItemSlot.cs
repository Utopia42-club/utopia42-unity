namespace src.Canvas
{
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
            stack = null;
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
            return ui;
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
            ui.SetItemSlot(null);
        }
    }

    public class ItemStack
    {
        public uint id;
        public int amount;

        public ItemStack(uint id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }
    }
}
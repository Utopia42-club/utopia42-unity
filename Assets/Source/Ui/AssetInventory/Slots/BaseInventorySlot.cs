using System;
using System.Collections;
using Source.Model.Inventory;
using Source.Ui.Utils;
using Source.UtopiaException;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Source.Ui.AssetInventory.Slots
{
    public abstract class BaseInventorySlot : UxmlElement, InventorySlot
    {
        public readonly VisualElement slotIcon;
        public readonly AssetsInventory assetsInventory;

        protected readonly Button leftAction;
        protected readonly Button rightAction;
        protected int size;
        protected int iconMargin;
        protected SlotInfo slotInfo;

        private IEnumerator imageCoroutine;
        private ToolTipManipulator toolTipManipulator;
        private readonly VisualElement selectedBorder;
        private bool selectable = true;
        private Action onSelect;
        private Texture2D loadedBackground;

        public BaseInventorySlot()
            : base(typeof(BaseInventorySlot))
        {
            assetsInventory = AssetsInventory.INSTANCE;
            slotIcon = this.Q<VisualElement>("slotIcon");
            leftAction = this.Q<Button>("leftAction");
            rightAction = this.Q<Button>("rightAction");

            selectedBorder = this.Q<VisualElement>("selectedBorder");

            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (selectable)
                {
                    if (onSelect != null)
                        onSelect();
                    else
                        assetsInventory.SelectSlot(this);
                }
            });
        }

        public void SetOnSelect(Action onSelect)
        {
            this.onSelect = onSelect;
        }

        public void SetSelectable(bool selectable)
        {
            this.selectable = selectable;
        }

        public void SetSize(int size, int iconMargin = 0)
        {
            this.size = size;
            this.iconMargin = iconMargin;
            style.width = size;
            style.height = size;
            style.marginBottom = style.marginTop = style.marginLeft = style.marginRight = 3;
            var ss = slotIcon.style;
            ss.marginBottom = ss.marginTop = ss.marginLeft = ss.marginRight = iconMargin;
        }

        public virtual void SetSlotInfo(SlotInfo slotInfo)
        {
            this.slotInfo = slotInfo;
        }

        public virtual SlotInfo GetSlotInfo()
        {
            return slotInfo;
        }

        public void SetTooltip(string tooltip)
        {
            slotIcon.tooltip = tooltip;
            if (tooltip == null && toolTipManipulator != null)
            {
                toolTipManipulator.Destroy();
                slotIcon.RemoveManipulator(toolTipManipulator);
            }
            else if (tooltip != null)
            {
                toolTipManipulator = new ToolTipManipulator();
                slotIcon.AddManipulator(toolTipManipulator);
            }
        }

        protected void LoadImage(string url)
        {
            imageCoroutine = UiImageUtils.SetBackGroundImageFromUrl(url, slotIcon, () =>
            {
                DestroyLoadedBackground();

                var texture = slotIcon.style.backgroundImage.value.texture;
                if (texture == null)
                    throw new IllegalStateException();
                loadedBackground = texture;
            });
            assetsInventory.StartCoroutine(imageCoroutine);
        }

        private void DestroyLoadedBackground()
        {
            if (loadedBackground != null)
            {
                Object.Destroy(loadedBackground);
                loadedBackground = null;
            }
        }

        public void SetBackground(Sprite sprite, bool destroyOnDetach)
        {
            DestroyLoadedBackground();
            UiImageUtils.SetBackground(slotIcon, sprite, destroyOnDetach);
        }

        public void HideSlotBackground()
        {
            this.Q<VisualElement>("slotContainer").style.backgroundImage = null;
        }

        public VisualElement VisualElement()
        {
            return this;
        }

        public void ConfigLeftAction(string tooltip = null, Sprite background = null, Action action = null)
        {
            leftAction.tooltip = tooltip;
            leftAction.style.backgroundImage = Background.FromSprite(background);
            leftAction.clickable = new Clickable(() => { });
            leftAction.clickable.clicked += () => action?.Invoke();
            leftAction.AddManipulator(new ToolTipManipulator());
        }

        public void SetLeftActionVisible(bool visible)
        {
            leftAction.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ConfigRightAction(string tooltip = null, Sprite background = null, Action action = null)
        {
            rightAction.tooltip = tooltip;
            rightAction.style.backgroundImage = Background.FromSprite(background);
            rightAction.clickable = new Clickable(() => { });
            rightAction.clickable.clicked += () => action?.Invoke();
            rightAction.AddManipulator(new ToolTipManipulator());
        }

        public void SetRightActionVisible(bool visible)
        {
            rightAction.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetSelected(bool selected)
        {
            if (selectedBorder != null)
                selectedBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public abstract object Clone();
    }
}
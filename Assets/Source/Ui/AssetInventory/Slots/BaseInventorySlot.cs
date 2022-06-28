using System;
using System.Collections;
using Source.Ui.AssetInventory.Models;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Slots
{
    public abstract class BaseInventorySlot : UxmlElement, InventorySlot
    {
        private static readonly Sprite assetDefaultImage = Resources.Load<Sprite>("Icons/loading");

        public readonly VisualElement slotIcon;
        public readonly AssetsInventory assetsInventory;

        protected readonly Button leftAction;
        protected readonly Button rightAction;
        protected int size;
        protected int iconMargin;
        protected SlotInfo slotInfo;

        private IEnumerator imageCoroutine;
        private bool isLoadingImage = false;
        private ToolTipManipulator toolTipManipulator;
        private readonly VisualElement selectedBorder;
        private bool selectable = true;
        private Action onSelect;

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

        public void SetGridPosition(int index, int itemsInARow)
        {
            GridUtils.SetChildPosition(this, size, size, index, itemsInARow);
        }

        protected void LoadImage(string url)
        {
            imageCoroutine = UiImageUtils.SetBackGroundImageFromUrl(url, assetDefaultImage,
                slotIcon, () => isLoadingImage = false);
            isLoadingImage = true;
            assetsInventory.StartCoroutine(imageCoroutine);
            slotIcon.RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                assetsInventory.StopCoroutine(imageCoroutine);
                isLoadingImage = false;
            });
        }

        public void SetBackground(Sprite sprite)
        {
            UiImageUtils.SetBackground(slotIcon, sprite);
        }

        public bool IsLoadingImage()
        {
            return isLoadingImage;
        }

        public void HideSlotBackground()
        {
            this.Q<VisualElement>("slotContainer").style.backgroundImage = null;
        }

        public VisualElement VisualElement()
        {
            return this;
        }

        public Sprite GetBackground()
        {
            return slotIcon.style.backgroundImage.value.sprite;
        }

        public void ConfigLeftAction(string tooltip = null, Sprite background = null,
            Action action = null)
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

        public void ConfigRightAction(string tooltip = null, Sprite background = null,
            Action action = null)
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
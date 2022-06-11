using System;
using System.Collections;
using src.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory.slots
{
    public abstract class BaseInventorySlot : InventorySlot
    {
        private static readonly Sprite assetDefaultImage = Resources.Load<Sprite>("Icons/loading");

        public readonly VisualElement slot;
        public readonly VisualElement slotIcon;
        public readonly AssetsInventory assetsInventory;

        protected readonly Button leftAction;
        protected int size;
        protected int iconMargin;

        private IEnumerator imageCoroutine;
        private bool isLoadingImage = false;
        private bool mouseDown;
        private ToolTipManipulator toolTipManipulator;

        public BaseInventorySlot()
        {
            assetsInventory = AssetsInventory.INSTANCE;
            slot = Resources.Load<VisualTreeAsset>("UiDocuments/InventorySlot").CloneTree();
            slotIcon = slot.Q<VisualElement>("slotIcon");
            leftAction = slot.Q<Button>("leftAction");
            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;
                mouseDown = true;
            });
            slot.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (mouseDown && evt.pressedButtons == 1)
                    assetsInventory.StartDrag(evt.position, this);
            });
            slot.RegisterCallback<PointerUpEvent>(evt => { mouseDown = false; });
        }

        public void SetSize(int size, int iconMargin = 0)
        {
            this.size = size;
            this.iconMargin = iconMargin;
            var s = slot.style;
            s.width = size;
            s.height = size;
            s.marginBottom = s.marginTop = s.marginLeft = s.marginRight = 3;
            var ss = slotIcon.style;
            ss.marginBottom = ss.marginTop = ss.marginLeft = ss.marginRight = iconMargin;
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
                toolTipManipulator = new ToolTipManipulator(assetsInventory.GetTooltipRoot());
                slotIcon.AddManipulator(toolTipManipulator);
            }
        }

        public void SetGridPosition(int index, int itemsInARow)
        {
            var s = slot.style;
            s.position = new StyleEnum<Position>(Position.Absolute);
            var div = index / itemsInARow;
            var rem = index % itemsInARow;
            s.left = rem * (size + 10);
            s.top = div * (size + 10);
        }

        protected void LoadImage(string url)
        {
            imageCoroutine = UiImageLoader.SetBackGroundImageFromUrl(url, assetDefaultImage,
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
            UiImageLoader.SetBackground(slotIcon, sprite);
        }

        public bool IsLoadingImage()
        {
            return isLoadingImage;
        }

        public void HideSlotBackground()
        {
            slot.Q<VisualElement>("slotContainer").style.backgroundImage = null;
        }

        public VisualElement VisualElement()
        {
            return slot;
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
            leftAction.clickable.clicked += () => action?.Invoke();
        }

        public void SetLeftActionVisible(bool visible)
        {
            leftAction.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public abstract object Clone();
    }
}
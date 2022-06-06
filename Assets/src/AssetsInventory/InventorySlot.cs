using src.AssetsInventory.Models;
using src.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory
{
    public class InventorySlot
    {
        private Sprite assetDefaultImage = Resources.Load<Sprite>("Icons/loading");
        private VisualElement slot;
        private VisualElement slotIcon;
        private Asset asset;
        private readonly global::AssetsInventory assetsInventory;
        private readonly VisualElement tooltipRoot;

        public InventorySlot(Asset asset, global::AssetsInventory assetsInventory, VisualElement tooltipRoot,
            int size = 80)
            : this(assetsInventory, tooltipRoot, asset.name, size)
        {
            SetAsset(asset);
        }

        public InventorySlot(global::AssetsInventory assetsInventory, VisualElement tooltipRoot = null,
            string tooltip = null, int size = 80, int iconMargin = 0)
        {
            this.assetsInventory = assetsInventory;
            this.tooltipRoot = tooltipRoot;
            slot = Resources.Load<VisualTreeAsset>("UiDocuments/InventorySlot").CloneTree();
            slotIcon = slot.Q<VisualElement>("slotIcon");
            SetTooltip(tooltip);
            var s = slot.style;
            s.width = size;
            s.height = size;
            s.marginBottom = s.marginTop = s.marginLeft = s.marginRight = 3;
            var ss = slotIcon.style;
            ss.marginBottom = ss.marginTop = ss.marginLeft = ss.marginRight = iconMargin;

            slot.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;

                assetsInventory.StartDrag(evt.position, this);
            });
        }

        private void SetTooltip(string tooltip)
        {
            if (tooltipRoot != null)
            {
                slotIcon.tooltip = tooltip;
                slotIcon.AddManipulator(new ToolTipManipulator(tooltipRoot));
            }
        }

        public void SetAsset(Asset asset)
        {
            this.asset = asset;
            var slotIcon = slot.Q<VisualElement>("slotIcon");
            var imageCoroutine =
                UiImageLoader.SetBackGroundImageFromUrl(asset.thumbnailUrl, assetDefaultImage, slotIcon);
            assetsInventory.StartCoroutine(imageCoroutine);
            slotIcon.RegisterCallback<DetachFromPanelEvent>(evt => { assetsInventory.StopCoroutine(imageCoroutine); });
            SetTooltip(asset.name);
        }

        public void SetBackground(Sprite sprite)
        {
            UiImageLoader.SetBackground(slotIcon, sprite);
        }

        public void SetBackground(string url)
        {
            assetsInventory.StartCoroutine(UiImageLoader.SetBackGroundImageFromUrl(url, assetDefaultImage, slotIcon));
        }

        public void SetGridPosition(int index, int itemsInARow)
        {
            var s = slot.style;
            s.position = new StyleEnum<Position>(Position.Absolute);
            var div = index / itemsInARow;
            var rem = index % itemsInARow;
            s.left = rem * 90;
            s.top = div * 90;
        }

        public void HideSlotBackground()
        {
            slot.Q<VisualElement>("slotContainer").style.backgroundImage = null;
        }

        public VisualElement VisualElement()
        {
            return slot;
        }

        public Asset GetAsset()
        {
            return asset;
        }
    }
}
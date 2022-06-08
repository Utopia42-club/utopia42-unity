using System.Collections;
using src.AssetsInventory.Models;
using src.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

namespace src.AssetsInventory
{
    public abstract class InventorySlot
    {
        private static readonly Sprite assetDefaultImage = Resources.Load<Sprite>("Icons/loading");

        private readonly VisualElement slot;
        private readonly VisualElement slotIcon;

        private readonly global::AssetsInventory assetsInventory;
        private readonly VisualElement tooltipRoot;
        private IEnumerator imageCoroutine;
        private bool isLoadingImage = false;

        protected InventorySlot(global::AssetsInventory assetsInventory, VisualElement tooltipRoot = null,
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

        protected void SetTooltip(string tooltip)
        {
            if (tooltipRoot != null)
            {
                slotIcon.tooltip = tooltip;
                slotIcon.AddManipulator(new ToolTipManipulator(tooltipRoot));
            }
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
    }

    public class AssetInventorySlot : InventorySlot
    {
        private Asset asset;

        public AssetInventorySlot(Asset asset, global::AssetsInventory assetsInventory,
            VisualElement tooltipRoot = null, int size = 80, int iconMargin = 0)
            : base(assetsInventory, tooltipRoot, asset.name, size, iconMargin)
        {
            SetAsset(asset);
        }

        public void SetAsset(Asset asset, bool updateImage = true)
        {
            this.asset = asset;
            SetTooltip(asset.name);
            if (updateImage)
                LoadImage(asset.thumbnailUrl);
        }

        public Asset GetAsset()
        {
            return asset;
        }
    }

    public class AssetBlockInventorySlot : InventorySlot
    {
        private Asset asset;

        public AssetBlockInventorySlot(global::AssetsInventory assetsInventory, VisualElement tooltipRoot = null,
            string tooltip = null, int size = 80, int iconMargin = 0)
            : base(assetsInventory, tooltipRoot, tooltip, size, iconMargin)
        {
        }

        public void SetAsset(Asset asset, bool updateImage = true)
        {
            this.asset = asset;
            SetTooltip(asset.name);
            if (updateImage)
                LoadImage(asset.thumbnailUrl);
        }

        public Asset GetAsset()
        {
            return asset;
        }

        public void UpdateSlot(InventorySlot inventorySlot)
        {
            switch (inventorySlot)
            {
                case AssetInventorySlot assetInventorySlot:
                    UpdateAssetSlot(assetInventorySlot, assetInventorySlot.GetAsset());
                    break;
                case AssetBlockInventorySlot assetBlockSlot:
                {
                    var asset = assetBlockSlot.GetAsset();
                    if (asset != null)
                    {
                        UpdateAssetSlot(inventorySlot, asset);
                    }

                    //TODO: else block
                    break;
                }
            }
        }

        private void UpdateAssetSlot(InventorySlot inventorySlot, Asset asset)
        {
            if (inventorySlot.IsLoadingImage())
            {
                SetAsset(asset);
            }
            else
            {
                SetAsset(asset, false);
                SetBackground(inventorySlot.GetBackground());
            }
        }
    }

    public class FavoriteItemInventorySlot : AssetBlockInventorySlot
    {
        public readonly FavoriteItem favoriteItem;

        public FavoriteItemInventorySlot(FavoriteItem favoriteItem, global::AssetsInventory assetsInventory,
            VisualElement tooltipRoot = null, string tooltip = null, int size = 80, int iconMargin = 0)
            : base(assetsInventory, tooltipRoot, tooltip, size, iconMargin)
        {
            this.favoriteItem = favoriteItem;
            if (favoriteItem?.asset != null)
            {
                SetAsset(favoriteItem.asset);
            }
            //TODO: else block
        }
    }
}
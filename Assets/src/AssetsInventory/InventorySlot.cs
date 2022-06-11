using System;
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

        protected readonly VisualElement slot;
        protected readonly VisualElement slotIcon;
        protected readonly Button leftAction;

        protected readonly global::src.AssetsInventory.AssetsInventory assetsInventory;
        private readonly VisualElement tooltipRoot;
        private IEnumerator imageCoroutine;
        private bool isLoadingImage = false;
        private bool mouseDown;

        protected InventorySlot(VisualElement tooltipRoot = null, string tooltip = null, int size = 80,
            int iconMargin = 0)
        {
            this.assetsInventory = global::src.AssetsInventory.AssetsInventory.INSTANCE;
            this.tooltipRoot = tooltipRoot;
            slot = Resources.Load<VisualTreeAsset>("UiDocuments/InventorySlot").CloneTree();
            slotIcon = slot.Q<VisualElement>("slotIcon");
            leftAction = slot.Q<Button>("leftAction");
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
                mouseDown = true;
            });
            slot.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (mouseDown && evt.pressedButtons == 1)
                    assetsInventory.StartDrag(evt.position, this);
            });
            slot.RegisterCallback<PointerUpEvent>(evt => { mouseDown = false; });
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

        protected void ConfigLeftAction(bool visible, string tooltip = null, Sprite background = null,
            Action action = null)
        {
            leftAction.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) return;
            leftAction.tooltip = tooltip;
            leftAction.style.backgroundImage = Background.FromSprite(background);
            leftAction.clickable.clicked += () => action?.Invoke();
        }
    }

    public class AssetInventorySlot : InventorySlot
    {
        private Asset asset;

        public AssetInventorySlot(Asset asset, VisualElement tooltipRoot = null, int size = 80, int iconMargin = 0)
            : base(tooltipRoot, asset.name, size, iconMargin)
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

        public AssetBlockInventorySlot(VisualElement tooltipRoot = null,
            string tooltip = null, int size = 80, int iconMargin = 0)
            : base(tooltipRoot, tooltip, size, iconMargin)
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
        private readonly Sprite closeIcon;
        private readonly VisualElement selectedBorder;

        public FavoriteItemInventorySlot(FavoriteItem favoriteItem,
            VisualElement tooltipRoot = null, string tooltip = null, int size = 80, int iconMargin = 0)
            : base(tooltipRoot, tooltip, size, iconMargin)
        {
            this.favoriteItem = favoriteItem;
            if (favoriteItem?.asset != null)
            {
                SetAsset(favoriteItem.asset);
            }
            //TODO: else block

            if (favoriteItem != null)
            {
                closeIcon = Resources.Load<Sprite>("Icons/close");
                slot.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    ConfigLeftAction(true, "Delete", closeIcon, () => assetsInventory.DeleteFavoriteItem(this));
                });
                slot.RegisterCallback<MouseLeaveEvent>(evt => ConfigLeftAction(false));

                selectedBorder = slot.Q<VisualElement>("selectedBorder");
                slot.RegisterCallback<PointerDownEvent>(evt => assetsInventory.SelectFavoriteItem(this));
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedBorder != null)
                selectedBorder.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
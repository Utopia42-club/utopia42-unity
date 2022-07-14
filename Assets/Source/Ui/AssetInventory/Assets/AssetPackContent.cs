using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using Source.Ui.AssetInventory.Slots;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory.Assets
{
    public class AssetPackContent : VisualElement
    {
        private readonly SearchCriteria searchCriteria;
        private readonly VisualElement loadingTarget;
        private readonly VisualElement slots;
        private readonly Label notFoundLabel;

        public AssetPackContent(SearchCriteria searchCriteria, Pack pack, VisualElement loadingTarget)
        {
            this.searchCriteria = searchCriteria;
            this.loadingTarget = loadingTarget;
            notFoundLabel = new Label("No items found")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = new StyleLength(new Length(100, LengthUnit.Percent)),
                    display = new StyleEnum<DisplayStyle>(DisplayStyle.None)
                }
            };
            Add(notFoundLabel);
            slots = new VisualElement();
            slots.AddToClassList("slots-wrapper");
            // slots.style.alignItems = Align.Center;
            slots.style.justifyContent = Justify.Center;
            Add(slots);
            searchCriteria.searchTerms.Add("pack", pack.id);
            var loadMore = Utils.Utils.Create("Ui/AssetInventory/Assets/LoadMoreButton");
            var loadMoreButton = loadMore.Q<Button>();
            loadMoreButton.clickable.clicked += () => Load();
            Add(loadMore);
            Load();
        }

        private void Load()
        {
            searchCriteria.limit = 15;
            if (slots.childCount > 0)
            {
                var slot = slots.ElementAt(slots.childCount - 1) as AssetInventorySlot;
                searchCriteria.lastId = slot.GetSlotInfo().asset.id.Value;
            }
            else
                searchCriteria.lastId = null;

            // searchCriteria.searchTerms.Clear();
            var loading = LoadingLayer.LoadingLayer.Show(loadingTarget); //FIXME
            var inventory = AssetsInventory.INSTANCE;
            inventory.StartCoroutine(inventory.restClient.GetAssets(searchCriteria, assets =>
            {
                var empty = childCount == 0 && assets.Count == 0;
                notFoundLabel.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
                if (!empty)
                    AddAssets(assets);
                else
                    style.height = 75;
                loading.Close();
            }, () => loading.Close()));
        }

        private void AddAssets(List<Asset> assets)
        {
            var count = slots.childCount;
            var size = assets.Count;
            for (var i = count; i < count + size; i++)
            {
                var slot = new AssetInventorySlot();
                var slotInfo = new SlotInfo(assets[i - count]);
                slot.SetSlotInfo(slotInfo);
                slot.SetSize(80);
                AssetsInventory.INSTANCE.SetupFavoriteAction(slot);
                slots.Add(slot);
            }
        }
    }
}
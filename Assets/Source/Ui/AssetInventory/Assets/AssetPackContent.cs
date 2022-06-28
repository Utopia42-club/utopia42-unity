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
        private readonly VisualElement slots;
        private readonly Label notFoundLabel;

        public AssetPackContent(SearchCriteria searchCriteria, Pack pack)
        {
            this.searchCriteria = searchCriteria;
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
            Add(slots);
            if (searchCriteria.searchTerms.ContainsKey("pack"))
                searchCriteria.searchTerms.Remove("pack");
            searchCriteria.searchTerms.Add("pack", pack);
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

            searchCriteria.searchTerms.Clear();
            var loading = LoadingLayer.LoadingLayer.Show(this); //FIXME
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
                slot.SetGridPosition(i, 3);
                // SetupFavoriteAction(slot);
                slots.Add(slot);
            }

            var total = size + count;
            GridUtils.SetContainerSize(slots, total, 90, 3);
            style.height = slots.style.height.value.value + 45;
        }
    }
}
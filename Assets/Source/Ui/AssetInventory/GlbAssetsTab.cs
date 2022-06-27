using System;
using System.Collections.Generic;
using Source.Ui.AssetInventory.Models;
using Source.Ui.TabPane;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.AssetInventory
{
    internal class GlbAssetsTab : UxmlElement, TabOpenListener
    {
        private readonly AssetsInventory inventory;
        private readonly AssetsRestClient restClient = new();
        private readonly Dictionary<int, Pack> packs = new();
        private readonly VisualElement breadcrumb;
        private string lastSearchFilter = "";
        private Category selectedCategory;
        private TabPane.TabPane tabPane;

        public GlbAssetsTab(AssetsInventory inventory)
            : base(typeof(GlbAssetsTab))
        {
            this.inventory = inventory;
            breadcrumb = this.Q("breadcrumb");
            Add(new Button(() =>
            {
                Debug.Log("Performing garbage collection...");
                GC.Collect();
            }) {text = "Perform GC"});
        }

        public void OnTabOpen(TabOpenEvent e)
        {
            tabPane = e.TabPane;
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            selectedCategory = null;
            var searchCriteria = new SearchCriteria
            {
                limit = 100
            };
            var loadingId = LoadingLayer.LoadingLayer.Show(tabPane); //FIXME target was inventory.content
            inventory.StartCoroutine(restClient.GetPacks(searchCriteria, packs =>
            {
                foreach (var pack in packs)
                    this.packs[pack.id] = pack;
                LoadingLayer.LoadingLayer.Hide(loadingId);
            }, () => LoadingLayer.LoadingLayer.Hide(loadingId)));

            var loadingId2 = LoadingLayer.LoadingLayer.Show(tabPane); //FIXME target was inventory.content
            inventory.StartCoroutine(restClient.GetCategories(searchCriteria, categories =>
            {
                var scrollView = this.Q<ScrollView>("categories");
                scrollView.Clear();
                Utils.Utils.IncreaseScrollSpeed(scrollView, 600);
                scrollView.mode = ScrollViewMode.Vertical;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
                foreach (var category in categories)
                    scrollView.Add(CreateCategoriesListItem(category));
                LoadingLayer.LoadingLayer.Hide(loadingId2);
            }, () => LoadingLayer.LoadingLayer.Hide(loadingId2)));

            var searchField = this.Q<TextField>("searchField");
            searchField.multiline = false;
            Utils.Utils.SetPlaceHolderForTextField(searchField, "Search");
            Utils.Utils.RegisterUiEngagementCallbacksForTextField(searchField);

            searchField.RegisterValueChangedCallback(new DebounceEventListener<ChangeEvent<string>>
                (inventory, 0.6f, e => FilterAssets(e.newValue)).Deligate);
        }

        private void FilterAssets(string filter)
        {
            Debug.Log("Filtering assets: " + filter);
            if (lastSearchFilter.Equals(filter))
                return;
            lastSearchFilter = filter;

            if (filter.Length == 0)
            {
                tabPane.OpenTab(0);
                return;
            }

            var sc = new SearchCriteria
            {
                limit = 100,
                searchTerms = new Dictionary<string, object>
                {
                    {"generalSearch", filter},
                }
            };
            if (selectedCategory != null)
                sc.searchTerms.Add("category", selectedCategory.id);
            var scrollView = CreateAssetsScrollView(sc, true);
            SetAssetsTabContent(scrollView, "Categories");
        }

        private VisualElement CreateCategoriesListItem(Category category)
        {
            var container = new VisualElement();
            var categoryButton = Utils.Utils.Create("Ui/AssetInventory/CategoryButton");
            container.style.paddingTop = container.style.paddingBottom = 3;
            container.Add(categoryButton);

            var label = categoryButton.Q<Label>("label");
            label.text = category.name;

            var image = categoryButton.Q("image");

            inventory.StartCoroutine(UiImageUtils.SetBackGroundImageFromUrl(category.thumbnailUrl,
                Resources.Load<Sprite>("Icons/loading"), image));

            categoryButton.Q<Button>().clickable.clicked += () =>
            {
                selectedCategory = category;
                var searchCriteria = new SearchCriteria
                {
                    limit = 100,
                    searchTerms = new Dictionary<string, object> {{"category", category.id}}
                };
                var scrollView = CreateAssetsScrollView(searchCriteria);
                SetAssetsTabContent(scrollView, category.name);
            };
            return container;
        }

        private void SetAssetsTabContent(VisualElement visualElement, string backButtonText = "Back")
        {
            var assetsTabContent = tabPane.GetTabBody().Q<VisualElement>("content");
            var categoriesView = tabPane.GetTabBody().Q<ScrollView>("categories");
            assetsTabContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            categoriesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            SetBodyContent(assetsTabContent, visualElement, backButtonText);
        }


        private ScrollView CreateAssetsScrollView(SearchCriteria searchCriteria, bool isSearchResult = false)
        {
            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                scrollDecelerationRate = 0.135f,
                verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };
            Utils.Utils.IncreaseScrollSpeed(scrollView, 600);
            var ss = scrollView.style;
            ss.height = new StyleLength(new Length(90, LengthUnit.Percent));
            ss.width = new StyleLength(new Length(90, LengthUnit.Percent));
            ss.flexGrow = 1;

            searchCriteria.limit = 20;

            foreach (var packEntry in packs)
            {
                var foldout = new PackFoldout<GlbPackContent>(packEntry.Value.name, true);

                // foldout.SetValueWithoutNotify(false);
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        foldout.SetContent(new GlbPackContent(inventory, searchCriteria, packEntry.Value));
                        // LoadAPageOfAssetsIntoFoldout(foldout, searchCriteria);
                    }
                });
                if (isSearchResult)
                    foldout.schedule.Execute(() => foldout.value = true);

                scrollView.Add(foldout);
            }

            return scrollView;
        }

        private void SetBodyContent(VisualElement body, VisualElement visualElement, string backButtonText = "Back")
        {
            body.Clear();
            // if (onBack == null)
            // {
            //     breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            //     body.Add(visualElement);
            // }
            // else
            // {
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            var backButton = Utils.Utils.Create("Ui/AssetInventory/BackButton");
            backButton.Q<Label>("label").text = backButtonText;
            backButton.Q<Button>().clickable.clicked += () => tabPane.OpenTab(0);
            backButton.style.width = 20 + backButtonText.Length * 10;
            breadcrumb.Clear();
            breadcrumb.Add(backButton);
            // }

            body.Add(visualElement);
        }


        private Dictionary<int, List<Asset>> GroupAssetsByPack(List<Asset> assets)
        {
            var dictionary = new Dictionary<int, List<Asset>>();
            foreach (var asset in assets)
            {
                var pack = packs[asset.pack.id];
                if (!dictionary.ContainsKey(pack.id))
                    dictionary[pack.id] = new List<Asset>();
                dictionary[pack.id].Add(asset);
            }

            return dictionary;
        }
    }
}
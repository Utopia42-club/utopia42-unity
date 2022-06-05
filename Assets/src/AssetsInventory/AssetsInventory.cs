using System;
using System.Collections.Generic;
using src.AssetsInventory;
using src.AssetsInventory.Models;
using src.Canvas;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AssetsInventory : MonoBehaviour
{
    private VisualElement root;
    private VisualElement tabBody;
    private Dictionary<int, Tuple<Button, string>> tabs;
    private readonly AssetsRestClient restClient = new AssetsRestClient();
    private int currentTab;
    private readonly Dictionary<int, Pack> packs = new Dictionary<int, Pack>();
    private VisualElement loadingLayer;
    private Category selectedCategory;
    private Sprite assetDefaultImage;
    private string filterText = "";

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        loadingLayer = root.Q<VisualElement>("loadingLayer");
        tabs = new Dictionary<int, Tuple<Button, string>>();

        tabBody = root.Q<VisualElement>("tabBody");
        var assetsTabBt = root.Q<Button>("assetsTab");
        var inventoryTabBt = root.Q<Button>("inventoryTab");

        tabs.Add(1, new Tuple<Button, string>(assetsTabBt, "UiDocuments/AssetsTab"));
        tabs.Add(2, new Tuple<Button, string>(inventoryTabBt, "UiDocuments/InventoryTab"));

        foreach (var tab in tabs)
            tab.Value.Item1.clicked += () => OpenTab(tab.Key);
        
        OpenTab(1);
    }

    private void Update()
    {
        if (filterText.Length > 0)
        {
            FilterAssets(filterText);
            filterText = "";
        }
    }

    private void ShowLoadingLayer(bool show)
    {
        loadingLayer.style.display =
            show ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    private void OpenTab(int index)
    {
        var (button, uxmlPath) = tabs[index];
        var tabBodyContent = Resources.Load<VisualTreeAsset>(uxmlPath).CloneTree();
        tabBodyContent.style.width = new StyleLength(new Length(95, LengthUnit.Percent));
        SetBodyContent(tabBody, tabBodyContent, null);
        foreach (var t in tabs)
            t.Value.Item1.RemoveFromClassList("selected-tab");
        button.AddToClassList("selected-tab");
        OnTabChanged(index);
    }

    private void OnTabChanged(int tabIndex)
    {
        currentTab = tabIndex;
        switch (tabIndex)
        {
            case 1:
                ShowAssetsTab();
                break;
            case 2:
                break;
        }
    }

    private void ShowAssetsTab()
    {
        selectedCategory = null;
        var searchCriteria = new SearchCriteria
        {
            limit = 100
        };
        ShowLoadingLayer(true);
        StartCoroutine(restClient.GetPacks(searchCriteria, packs =>
        {
            foreach (var pack in packs)
                this.packs[pack.id] = pack;
        }, () => { ShowLoadingLayer(false); }));

        assetDefaultImage = Resources.Load<Sprite>("Icons/loading");
        ShowLoadingLayer(true);
        StartCoroutine(restClient.GetCategories(searchCriteria, categories =>
        {
            if (currentTab != 1)
                return;
            var listView = tabBody.Q<ListView>("categories");
            var scrollView = listView.Q<ScrollView>();
            scrollView.mode = ScrollViewMode.Vertical;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            listView.makeItem = CreateCategoriesListViewItem;
            listView.bindItem = (item, index) => CategoriesListViewBindItem(item, categories, index);
            listView.itemsSource = categories;
            listView.Rebuild();
            ShowLoadingLayer(false);
        }, () => { ShowLoadingLayer(false); }));

        // Setup searchField
        var searchField = tabBody.Q<TextField>("searchField");
        searchField.RegisterCallback<FocusInEvent>(evt =>
        {
            if (searchField.text == "Search")
                searchField.SetValueWithoutNotify("");
        });
        searchField.RegisterCallback<FocusOutEvent>(evt =>
        {
            if (searchField.text == "")
                searchField.SetValueWithoutNotify("Search");
        });
        var debounce = Utils.Debounce<string>(arg => filterText = arg);
        searchField.RegisterValueChangedCallback(evt => { debounce(evt.newValue); });
    }

    private void FilterAssets(string filter)
    {
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
        var scrollView = CreateAssetsScrollView(sc);
        SetAssetsTabContent(scrollView, () => OpenTab(1), "Categories");
    }

    private static VisualElement CreateCategoriesListViewItem()
    {
        var container = new VisualElement();
        var button = Resources.Load<VisualTreeAsset>("UiDocuments/CategoryButton")
            .CloneTree();
        container.style.paddingTop = container.style.paddingBottom = 3;
        container.Add(button);
        return container;
    }

    private void CategoriesListViewBindItem(VisualElement item, List<Category> categories, int index)
    {
        var button = item.Q<Button>();
        var label = item.Q<Label>("label");
        var category = categories[index];
        label.text = category.name;

        var image = item.Q("image");

        StartCoroutine(UiImageLoader.SetBackGroundImageFromUrl(category.thumbnailUrl,
            Resources.Load<Sprite>("Icons/loading"), image));

        button.clickable.clicked += () =>
        {
            selectedCategory = category;
            var searchCriteria = new SearchCriteria
            {
                limit = 100,
                searchTerms = new Dictionary<string, object> {{"category", category.id}}
            };
            var scrollView = CreateAssetsScrollView(searchCriteria);
            SetAssetsTabContent(scrollView, () => OpenTab(1), "Categories");
        };
    }

    private void SetAssetsTabContent(VisualElement visualElement, Action onBack, string backButtonText = "Back")
    {
        var assetsTabContent = tabBody.Q<VisualElement>("content");
        var listView = tabBody.Q<ListView>("categories");
        assetsTabContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        listView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        SetBodyContent(assetsTabContent, visualElement, onBack, backButtonText);
    }


    private ScrollView CreateAssetsScrollView(SearchCriteria searchCriteria)
    {
        var scrollView = new ScrollView(ScrollViewMode.Vertical)
        {
            scrollDecelerationRate = 0.135f,
            verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible
        };
        scrollView.RegisterCallback<WheelEvent>(evt => { evt.StopPropagation(); });
        Utils.IncreaseScrollSpeed(scrollView, 600);
        scrollView.AddToClassList("utopia-scrollView");
        var ss = scrollView.style;
        ss.height = new StyleLength(new Length(90, LengthUnit.Percent));
        ss.width = new StyleLength(new Length(90, LengthUnit.Percent));
        ss.flexGrow = 1;

        ShowLoadingLayer(true);
        StartCoroutine(restClient.GetAllAssets(searchCriteria, assets =>
        {
            var assetGroups = GroupAssetsByPack(assets);
            foreach (var assetGroup in assetGroups)
            {
                var foldout = new Foldout
                {
                    text = packs[assetGroup.Key].name
                };
                foldout.SetValueWithoutNotify(true);
                foldout.AddToClassList("utopia-foldout");
                var fs = foldout.style;
                fs.marginRight = fs.marginLeft = fs.marginBottom = fs.marginTop = 5;
                var size = assetGroup.Value.Count;
                for (var i = 0; i < size; i++)
                {
                    var slot = CreateSlot(assetGroup.Value[i], i);
                    foldout.contentContainer.Add(slot);
                }

                foldout.contentContainer.style.height = 90 * (size / 3 + 1);
                scrollView.Add(foldout);
            }

            ShowLoadingLayer(false);
        }, () => { ShowLoadingLayer(false); }, this));
        return scrollView;
    }

    private void SetBodyContent(VisualElement body, VisualElement visualElement, Action onBack,
        string backButtonText = "Back")
    {
        body.Clear();
        var breadcrumb = root.Q("breadcrumb");
        if (onBack == null)
        {
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            body.Add(visualElement);
        }
        else
        {
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            var backButton = Resources.Load<VisualTreeAsset>("UiDocuments/BackButton")
                .CloneTree();
            backButton.Q<Label>("label").text = backButtonText;
            backButton.Q<Button>().clickable.clicked += onBack;
            backButton.style.width = 20 + backButtonText.Length * 10;
            breadcrumb.Clear();
            breadcrumb.Add(backButton);
        }

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

    private VisualElement CreateSlot(Asset asset, int i)
    {
        var slot = Resources.Load<VisualTreeAsset>("UiDocuments/InventorySlot").CloneTree();
        var slotIcon = slot.Q<VisualElement>("slotIcon");
        slotIcon.tooltip = asset.name;
        slotIcon.AddManipulator(new ToolTipManipulator(root));
        var imageCoroutine = UiImageLoader.SetBackGroundImageFromUrl(asset.thumbnailUrl, assetDefaultImage, slotIcon);
        StartCoroutine(imageCoroutine);
        slotIcon.RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            StopCoroutine(imageCoroutine);
        });
        var s = slot.style;
        s.width = 80;
        s.height = 80;
        s.marginBottom = s.marginTop = s.marginLeft = s.marginRight = 3;
        s.position = new StyleEnum<Position>(Position.Absolute);
        var div = i / 3;
        var rem = i % 3;
        s.left = rem * 90;
        s.top = div * 90;
        return slot;
    }
}
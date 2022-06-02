using System;
using System.Collections;
using System.Collections.Generic;
using Nethereum.Model;
using src.AssetsInventory;
using src.AssetsInventory.Models;
using src.Canvas;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AssetsInventory : MonoBehaviour
{
    private VisualElement root;
    private VisualElement tabBody;
    private Dictionary<int, Tuple<Button, string>> tabs;
    private AssetsRestClient restClient = new AssetsRestClient();
    private int currentTab;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        tabs = new Dictionary<int, Tuple<Button, string>>();

        tabBody = root.Q<VisualElement>("tabBody");
        var assetsTabBt = root.Q<Button>("assetsTab");
        var inventoryTabBt = root.Q<Button>("inventoryTab");

        tabs.Add(1, new Tuple<Button, string>(assetsTabBt, "UiDocuments/AssetsTab"));
        tabs.Add(2, new Tuple<Button, string>(inventoryTabBt, "UiDocuments/InventoryTab"));

        foreach (var tab in tabs)
            tab.Value.Item1.clicked += () => OpenTab(tab.Key);
    }

    private void OpenTab(int index)
    {
        var (button, uxmlPath) = tabs[index];
        var tabBodyContent = Resources.Load<VisualTreeAsset>(uxmlPath).CloneTree();
        tabBodyContent.style.width = new StyleLength(new Length(95, LengthUnit.Percent));
        SetTabBodyContent(tabBodyContent, null);
        foreach (var t in tabs)
            t.Value.Item1.RemoveFromClassList("selected-tab");
        button.AddToClassList("selected-tab");
        OnTabChanged(index);
    }

    void OnTabChanged(int tabIndex)
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
        var searchCriteria = new SearchCriteria();
        searchCriteria.limit = 100;
        StartCoroutine(restClient.GetCategories(searchCriteria, categories =>
        {
            if (currentTab != 1)
                return;
            var assetDefaultImage = Resources.Load<Sprite>("Icons/loading");
            var listView = tabBody.Q<ListView>("categories");
            var scrollView = listView.Q<ScrollView>();
            scrollView.mode = ScrollViewMode.Vertical;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            listView.makeItem = CreateListViewItem;
            listView.bindItem = (item, index) => ListViewBindItem(item, categories, index, assetDefaultImage);
            listView.itemsSource = categories;
            listView.Rebuild();
        }, () => { }));
    }

    private void ListViewBindItem(VisualElement item, List<Category> categories, int index, Sprite assetDefaultImage)
    {
        var button = item.Q<Button>();
        var label = item.Q<Label>("label");
        var category = categories[index];
        label.text = category.name;

        var image = item.Q("image");
        StartCoroutine(UiImageLoader.SetBackGroundImageFromUrl(category.thumbnailUrl,
            Resources.Load<Sprite>("Icons/loading"), image));

        item.userData = category;
        button.clickable.clicked += () =>
        {
            var scrollView = CreateAssetsScrollView(assetDefaultImage, label, category);
            SetTabBodyContent(scrollView, () => OpenTab(1), "Categories");
        };
    }

    private void SetTabBodyContent(VisualElement visualElement, Action onBack, string backButtonText = "Back")
    {
        tabBody.Clear();
        var breadcrumb = root.Q("breadcrumb");
        if (onBack == null)
        {
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            tabBody.Add(visualElement);
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

        tabBody.Add(visualElement);
    }


    private ScrollView CreateAssetsScrollView(Sprite assetDefaultImage, Label label, Category category)
    {
        var scrollView = new ScrollView(ScrollViewMode.Vertical);
        scrollView.scrollDecelerationRate = 0.135f;
        scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        scrollView.RegisterCallback<WheelEvent>(evt => { evt.StopPropagation(); });
        IncreaseScrollSpeed(scrollView, 600);
        scrollView.AddToClassList("utopia-scrollView");
        var ss = scrollView.style;
        ss.height = new StyleLength(new Length(90, LengthUnit.Percent));
        ss.width = new StyleLength(new Length(90, LengthUnit.Percent));
        label.contentContainer.Add(scrollView);
        var searchCriteria = new SearchCriteria();
        searchCriteria.limit = 100;
        searchCriteria.searchTerms = new Dictionary<string, object>();
        searchCriteria.searchTerms.Add("category", category.id);
        StartCoroutine(restClient.GetAllAssets(searchCriteria, assets =>
        {
            var visualElement = new VisualElement();
            visualElement.style.height = 300;
            for (var i = 0; i < assets.Count; i++)
            {
                var slot = CreateSlot(assets, i, assetDefaultImage);
                visualElement.Add(slot);
            }

            scrollView.Add(visualElement);
        }, () => { }, this));
        return scrollView;
    }

    private static VisualElement CreateListViewItem()
    {
        var container = new VisualElement();
        var button = Resources.Load<VisualTreeAsset>("UiDocuments/CategoryButton")
            .CloneTree();
        container.style.paddingTop = container.style.paddingBottom = 3;
        container.Add(button);
        return container;
    }

    private TemplateContainer CreateSlot(List<Asset> assets, int i, Sprite assetDefaultImage)
    {
        var asset = assets[i];
        var slot = Resources.Load<VisualTreeAsset>("UiDocuments/InventorySlot")
            .CloneTree();
        var slotIcon = slot.Q<VisualElement>("slotIcon");
        StartCoroutine(
            UiImageLoader.SetBackGroundImageFromUrl(asset.thumbnailUrl,
                assetDefaultImage, slotIcon)
        );
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

    private static void IncreaseScrollSpeed(ScrollView scrollView, float factor)
    {
        //Workaround to increase scroll speed...
        //There is this issue that verticalPageSize has no effect on speed
        scrollView.RegisterCallback<WheelEvent>((evt) =>
        {
            scrollView.scrollOffset = new Vector2(0,
                scrollView.scrollOffset.y + factor * evt.delta.y);
            evt.StopPropagation();
        });
    }
}
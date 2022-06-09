using System;
using System.Collections.Generic;
using System.Linq;
using src.AssetsInventory;
using src.AssetsInventory.Models;
using src.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

public class AssetsInventory : MonoBehaviour
{
    private VisualElement root;
    private VisualElement tabBody;
    private VisualElement loadingLayer;
    private VisualElement inventory;
    private VisualElement favPanel;
    private Button openCloseInvButton;
    private ScrollView favBar;

    private Sprite closeImage;
    private Sprite openImage;

    private Dictionary<int, Tuple<Button, string>> tabs;
    private readonly AssetsRestClient restClient = new();
    private int currentTab;
    private readonly Dictionary<int, Pack> packs = new();
    private Category selectedCategory;
    private string filterText = "";
    private static bool isDragging;

    private List<FavoriteItemInventorySlot> favBarSlots = new();
    private FavoriteItemInventorySlot addSlot;
    private AssetBlockInventorySlot ghostSlot;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        inventory = root.Q<VisualElement>("inventory");
        favPanel = root.Q<VisualElement>("favPanel");
        favBar = favPanel.Q<ScrollView>("favBar");
        Utils.IncreaseScrollSpeed(favBar, 600);
        openCloseInvButton = favPanel.Q<Button>("openCloseInvButton");
        openCloseInvButton.clickable.clicked += ToggleInventory;
        loadingLayer = root.Q<VisualElement>("loadingLayer");
        ghostSlot = new AssetBlockInventorySlot(this, null, null, 60);
        root.Add(ghostSlot.VisualElement());
        ghostSlot.VisualElement().visible = false;
        ghostSlot.HideSlotBackground();

        openImage = Resources.Load<Sprite>("Icons/openPane");
        closeImage = Resources.Load<Sprite>("Icons/closePane");

        tabs = new Dictionary<int, Tuple<Button, string>>();
        tabBody = root.Q<VisualElement>("tabBody");
        var assetsTabBt = root.Q<Button>("assetsTab");
        var blocksTabBt = root.Q<Button>("blocksTab");

        tabs.Add(1, new Tuple<Button, string>(assetsTabBt, "UiDocuments/AssetsTab"));
        tabs.Add(2, new Tuple<Button, string>(blocksTabBt, "UiDocuments/BlocksTab"));

        foreach (var tab in tabs)
            tab.Value.Item1.clicked += () => OpenTab(tab.Key);

        ghostSlot.VisualElement().RegisterCallback<PointerUpEvent>(GhostSlotOnMouseUp);

        StartCoroutine(restClient.GetAllFavoriteItems(new SearchCriteria(), favItems =>
        {
            addSlot = new FavoriteItemInventorySlot(null, this, root, "Add", 70, 10);
            addSlot.SetBackground(Resources.Load<Sprite>("Icons/add"));
            favBarSlots.Add(addSlot);
            favBar.Add(addSlot.VisualElement());

            foreach (var favoriteItem in favItems)
                AddToFavoritePanel(favoriteItem);
        }, () => { }, this));
    }

    private void AddToFavoritePanel(FavoriteItem favoriteItem)
    {
        var slot = new FavoriteItemInventorySlot(favoriteItem, this, size: 70);
        favBarSlots.Add(slot);
        favBar.RemoveAt(favBar.childCount - 1);
        favBar.Add(slot.VisualElement());
        favBar.Add(addSlot.VisualElement());
    }

    private void RemoveFromFavoritePanel(FavoriteItemInventorySlot slot)
    {
        favBarSlots.Remove(slot);
        favBar.Remove(slot.VisualElement());
    }

    public void DeleteFavoriteItem(FavoriteItemInventorySlot slot)
    {
        StartCoroutine(restClient.DeleteFavoriteItem(slot.favoriteItem.id.Value,
            () => RemoveFromFavoritePanel(slot), () => { }));
    }

    private void ToggleInventory()
    {
        var isVisible = inventory.style.visibility == Visibility.Visible;
        inventory.style.visibility = isVisible ? Visibility.Hidden : Visibility.Visible;
        favPanel.style.right = isVisible ? 5 : 362;
        var background = new StyleBackground
        {
            value = Background.FromSprite(isVisible ? openImage : closeImage)
        };
        openCloseInvButton.style.backgroundImage = background;
        if (!isVisible)
            OpenTab(1);
    }

    private void Update()
    {
        if (filterText.Length > 0)
        {
            FilterAssets(filterText);
            filterText = "";
        }

        if (isDragging)
        {
            var pos = Input.mousePosition;
            ghostSlot.VisualElement().style.top =
                root.worldBound.height - pos.y - ghostSlot.VisualElement().layout.height / 2;
            ghostSlot.VisualElement().style.left = pos.x - ghostSlot.VisualElement().layout.width / 2;
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
        currentTab = index;
        switch (index)
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

        ShowLoadingLayer(true);
        StartCoroutine(restClient.GetCategories(searchCriteria, categories =>
        {
            if (currentTab != 1)
                return;
            var scrollView = tabBody.Q<ScrollView>("categories");
            scrollView.Clear();
            Utils.IncreaseScrollSpeed(scrollView, 600);
            scrollView.mode = ScrollViewMode.Vertical;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            foreach (var category in categories)
                scrollView.Add(CreateCategoriesListItem(category));

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

    private VisualElement CreateCategoriesListItem(Category category)
    {
        var container = new VisualElement();
        var categoryButton = Resources.Load<VisualTreeAsset>("UiDocuments/CategoryButton").CloneTree();
        container.style.paddingTop = container.style.paddingBottom = 3;
        container.Add(categoryButton);

        var label = categoryButton.Q<Label>("label");
        label.text = category.name;

        var image = categoryButton.Q("image");

        StartCoroutine(UiImageLoader.SetBackGroundImageFromUrl(category.thumbnailUrl,
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
            SetAssetsTabContent(scrollView, () => OpenTab(1), "Categories");
        };
        return container;
    }

    private void SetAssetsTabContent(VisualElement visualElement, Action onBack, string backButtonText = "Back")
    {
        var assetsTabContent = tabBody.Q<VisualElement>("content");
        var categoriesView = tabBody.Q<ScrollView>("categories");
        assetsTabContent.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        categoriesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
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
                    var slot = new AssetInventorySlot(assetGroup.Value[i], this, root);
                    slot.SetGridPosition(i, 3);
                    foldout.contentContainer.Add(slot.VisualElement());
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

    public void StartDrag(Vector2 position, InventorySlot slot)
    {
        isDragging = true;

        ghostSlot.VisualElement().style.top = position.y - ghostSlot.VisualElement().layout.height / 2;
        ghostSlot.VisualElement().style.left = position.x - ghostSlot.VisualElement().layout.width / 2;

        ghostSlot.UpdateSlot(slot);

        ghostSlot.VisualElement().style.visibility = Visibility.Visible;
    }

    private void GhostSlotOnMouseUp(PointerUpEvent evt)
    {
        if (!isDragging)
            return;

        var slots = favBarSlots
            .Where(favBarSlot =>
                favBarSlot.VisualElement().worldBound.Overlaps(ghostSlot.VisualElement().worldBound))
            .ToList();

        if (slots.Count != 0)
        {
            var closestSlot = slots.OrderBy(x => Vector2.Distance
                (x.VisualElement().worldBound.position, ghostSlot.VisualElement().worldBound.position)).First();
            if (closestSlot == addSlot)
            {
                var favoriteItem = new FavoriteItem
                {
                    asset = new Asset
                    {
                        id = ghostSlot.GetAsset().id
                    }
                    //TODO: block
                };
                StartCoroutine(restClient.CreateFavoriteItem(favoriteItem, AddToFavoritePanel, () => { }));
            }
            else
            {
                closestSlot.UpdateSlot(ghostSlot);
                closestSlot.favoriteItem.asset = ghostSlot.GetAsset();
                //TODO: block
                StartCoroutine(restClient.UpdateFavoriteItem(closestSlot.favoriteItem, () => { }, () => { }));
            }
        }

        isDragging = false;
        ghostSlot.VisualElement().style.visibility = Visibility.Hidden;
    }
}
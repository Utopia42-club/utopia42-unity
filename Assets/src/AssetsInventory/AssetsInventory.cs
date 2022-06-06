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
    private Dictionary<int, Tuple<Button, string>> tabs;
    private readonly AssetsRestClient restClient = new AssetsRestClient();
    private int currentTab;
    private readonly Dictionary<int, Pack> packs = new Dictionary<int, Pack>();
    private VisualElement loadingLayer;
    private Category selectedCategory;
    private string filterText = "";
    private static bool isDragging;
    private VisualElement inventory;
    private VisualElement favPanel;
    private Button openCloseInvButton;
    private Sprite closeImage;
    private Sprite openImage;
    private ScrollView favBar;
    private List<InventorySlot> favBarSlots = new List<InventorySlot>();
    private InventorySlot addSlot;
    private InventorySlot ghostSlot;

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
        ghostSlot = new InventorySlot(this, null, null, 60);
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

        ghostSlot.VisualElement().RegisterCallback<PointerMoveEvent>(GhostSlotOnMouseMove);
        ghostSlot.VisualElement().RegisterCallback<PointerUpEvent>(GhostSlotOnMouseUp);

        StartCoroutine(restClient.GetAllFavoriteItems(new SearchCriteria(), favItems =>
        {
            foreach (var favoriteItem in favItems)
            {
                if (favoriteItem.asset != null)
                {
                    var slot = new InventorySlot(favoriteItem.asset, this, root, 70);
                    favBarSlots.Add(slot);
                    favBar.Add(slot.VisualElement());
                }
                else
                {
                    //TODO: implement
                }
            }

            addSlot = new InventorySlot(this, root, "Add", 80, 10);
            addSlot.SetBackground(Resources.Load<Sprite>("Icons/add"));
            favBarSlots.Add(addSlot);
            favBar.Add(addSlot.VisualElement());
        }, () => { }, this));
    }

    private void GhostSlotOnMouseUp(PointerUpEvent evt)
    {
        if (!isDragging)
            return;

        var slots
            = favBarSlots.Where(favBarSlot =>
                    favBarSlot.VisualElement().worldBound.Overlaps(ghostSlot.VisualElement().worldBound))
                .ToList();


        //Found at least one
        if (slots.Count != 0)
        {
            InventorySlot closestSlot = slots.OrderBy(x => Vector2.Distance
                (x.VisualElement().worldBound.position, ghostSlot.VisualElement().worldBound.position)).First();

            //Set the new inventory slot with the data
            closestSlot.SetAsset(ghostSlot.GetAsset());
        }

        //Clear dragging related visuals and data
        isDragging = false;
        ghostSlot.VisualElement().style.visibility = Visibility.Hidden;
    }

    private void GhostSlotOnMouseMove(PointerMoveEvent evt)
    {
        if (!isDragging)
            return;
        ghostSlot.VisualElement().style.top = evt.position.y - ghostSlot.VisualElement().layout.height / 2;
        ghostSlot.VisualElement().style.left = evt.position.x - ghostSlot.VisualElement().layout.width / 2;
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
            var listView = tabBody.Q<ListView>("categories");
            var scrollView = listView.Q<ScrollView>();
            Utils.IncreaseScrollSpeed(scrollView, 600);
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
                    var slot = new InventorySlot(assetGroup.Value[i], this, root);
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
        //Set tracking variables
        isDragging = true;

        //Set the new position
        ghostSlot.VisualElement().style.top = position.y - ghostSlot.VisualElement().layout.height / 2;
        ghostSlot.VisualElement().style.left = position.x - ghostSlot.VisualElement().layout.width / 2;

        //Set the image
        ghostSlot.SetBackground(slot.GetAsset().thumbnailUrl);

        //Flip the visibility on
        ghostSlot.VisualElement().style.visibility = Visibility.Visible;
    }
}
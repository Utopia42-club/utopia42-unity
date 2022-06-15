using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.AssetsInventory.Models;
using src.AssetsInventory.slots;
using src.Canvas;
using src.MetaBlocks;
using src.Model;
using src.UiUtils;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace src.AssetsInventory
{
    public class AssetsInventory : MonoBehaviour
    {
        private static AssetsInventory instance;
        private static readonly string HANDY_SLOTS_KEY = "HANDY_SLOTS";

        private VisualElement root;
        private VisualElement inventoryLoadingLayer;
        private VisualElement inventory;
        private VisualElement handyPanel;
        private Button openCloseInvButton;
        private ScrollView handyBar;

        private Sprite closeIcon;
        private Sprite openIcon;

        private Sprite addToFavoriteIcon;
        private Sprite removeFromFavoriteIcon;

        private readonly AssetsRestClient restClient = new();
        private readonly Dictionary<int, Pack> packs = new();
        private Category selectedCategory;
        private string filterText = "";

        private List<InventorySlotWrapper> handyBarSlots = new();

        private InventorySlot selectedSlot;
        public readonly UnityEvent<SlotInfo> selectedSlotChanged = new();

        [SerializeField] private ColorSlotPicker colorSlotPicker;
        private Foldout colorBlocksFoldout;
        private List<FavoriteItem> favoriteItems;
        private TabPane tabPane;
        private VisualElement breadcrumb;
        private VisualElement inventoryContainer;
        private bool firstTime = true;

        void Start()
        {
            openIcon = Resources.Load<Sprite>("Icons/openPane");
            closeIcon = Resources.Load<Sprite>("Icons/closePane");

            addToFavoriteIcon = Resources.Load<Sprite>("Icons/whiteHeart");
            removeFromFavoriteIcon = Resources.Load<Sprite>("Icons/redHeart");

            GameManager.INSTANCE.stateChange.AddListener(_ => UpdateVisibility());
            Player.INSTANCE.viewModeChanged.AddListener(_ => UpdateVisibility());
            UpdateVisibility();

            Player.INSTANCE.InitOnSelectedAssetChanged(); // TODO ?
        }

        void OnEnable()
        {
            if (firstTime)
            {
                firstTime = false;
                instance = this;
                PlayerPrefs.SetString(HANDY_SLOTS_KEY, "[]");
            }

            root = GetComponent<UIDocument>().rootVisualElement;
            inventory = root.Q<VisualElement>("inventory");
            inventoryContainer = root.Q<VisualElement>("inventoryContainer");

            var tabConfigurations = new List<TabConfiguration>
            {
                new("Assets", "UiDocuments/AssetsTab", SetupAssetsTab),
                new("Blocks", "UiDocuments/BlocksTab", SetupBlocksTab),
                new("Favorites", "UiDocuments/FavoritesTab", SetupFavoritesTab)
            };
            tabPane = new TabPane(tabConfigurations);
            var s = tabPane.VisualElement().style;
            s.height = new StyleLength(new Length(95, LengthUnit.Percent));
            s.width = 350;
            inventory.Add(tabPane.VisualElement());
            inventoryLoadingLayer = root.Q<VisualElement>("tabPaneLoadingLayer");

            breadcrumb = root.Q("breadcrumb");

            handyPanel = root.Q<VisualElement>("handyPanel");
            handyBar = handyPanel.Q<ScrollView>("handyBar");
            Utils.IncreaseScrollSpeed(handyBar, 600);
            openCloseInvButton = handyPanel.Q<Button>("openCloseInvButton");
            openCloseInvButton.clickable.clicked += ToggleInventory;

            handyBarSlots.Clear();
            handyBar.Clear();
            foreach (var savedHandySlot in GetSavedHandySlots())
                AddToHandyPanel(savedHandySlot);
        }

        private void UpdateVisibility()
        {
            var active = GameManager.INSTANCE.GetState() == GameManager.State.PLAYING
                         && Player.INSTANCE.GetViewMode() == Player.ViewMode.FIRST_PERSON
                ; // && Can Edit Land 
            gameObject.SetActive(active);
            inventoryContainer.style.visibility = Visibility.Visible; // is null at start and can't be checked !
            ToggleInventory();
            if (active)
                LoadFavoriteItems(); // FIXME: what to do on error?
        }

        private void SetupAssetsTab()
        {
            selectedCategory = null;
            var searchCriteria = new SearchCriteria
            {
                limit = 100
            };
            ShowInventoryLoadingLayer(true);
            StartCoroutine(restClient.GetPacks(searchCriteria, packs =>
            {
                foreach (var pack in packs)
                    this.packs[pack.id] = pack;
            }, () => ShowInventoryLoadingLayer(false)));

            ShowInventoryLoadingLayer(true);
            StartCoroutine(restClient.GetCategories(searchCriteria, categories =>
            {
                var scrollView = tabPane.GetTabBody().Q<ScrollView>("categories");
                scrollView.Clear();
                Utils.IncreaseScrollSpeed(scrollView, 600);
                scrollView.mode = ScrollViewMode.Vertical;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
                foreach (var category in categories)
                    scrollView.Add(CreateCategoriesListItem(category));

                ShowInventoryLoadingLayer(false);
            }, () => { ShowInventoryLoadingLayer(false); }));

            // Setup searchField
            var searchField = tabPane.GetTabBody().Q<TextField>("searchField");
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
            searchField.RegisterValueChangedCallback(evt => debounce(evt.newValue));
        }


        private void SetupBlocksTab()
        {
            var scrollView = tabPane.GetTabBody().Q<ScrollView>("blockPacks");
            scrollView.Clear();
            Utils.IncreaseScrollSpeed(scrollView, 600);
            scrollView.mode = ScrollViewMode.Vertical;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

            var regularBlocksFoldout = CreateBlocksPackFoldout("Regular Blocks",
                Blocks.GetBlockTypes().Where(blockType => blockType is not MetaBlockType && blockType.name != "air")
                    .ToList());

            var metaBlocksFoldout = CreateBlocksPackFoldout("Meta Blocks",
                Blocks.GetBlockTypes().Where(blockType => blockType is MetaBlockType).ToList());

            colorSlotPicker.SetOnColorCreated(color =>
            {
                ColorBlocks.SaveBlockColor(color);
                UpdateUserColorBlocks();
            });
            colorSlotPicker.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

            scrollView.Add(regularBlocksFoldout);
            scrollView.Add(metaBlocksFoldout);

            colorBlocksFoldout = CreateColorBlocksFoldout();
            UpdateUserColorBlocks();
            scrollView.Add(colorBlocksFoldout);
        }

        private void SetupFavoritesTab()
        {
            LoadFavoriteItems(() =>
            {
                var scrollView = tabPane.GetTabBody().Q<ScrollView>("favorites");
                var container = new VisualElement();
                for (int i = 0; i < favoriteItems.Count; i++)
                {
                    var favoriteItem = favoriteItems[i];
                    var slot = new FavoriteItemInventorySlot(favoriteItem);
                    Utils.SetGridPosition(slot.VisualElement(), 80, i, 3);
                    container.Add(slot.VisualElement());
                }

                Utils.SetGridContainerSize(container, favoriteItems.Count);
                scrollView.Add(container);
            }, () =>
            {
                //TODO: show error snack
            });
        }

        private void Update()
        {
            if (filterText.Length > 0)
            {
                FilterAssets(filterText);
                filterText = "";
            }
        }

        public void AddToHandyPanel(SlotInfo slotInfo)
        {
            foreach (var handyBarSlot in handyBarSlots)
            {
                if (handyBarSlot.GetSlotInfo().Equals(slotInfo))
                {
                    handyBarSlots.Remove(handyBarSlot);
                    handyBarSlots.Insert(0, handyBarSlot);
                    handyBar.Remove(handyBarSlot.VisualElement());
                    handyBar.Insert(0, handyBarSlot.VisualElement());
                    SelectSlot(handyBarSlot, false);
                    SaveHandySlots();
                    return;
                }
            }

            var slot = new HandyItemInventorySlot();
            slot.SetSize(70);
            slot.SetSlotInfo(slotInfo);
            SelectSlot(slot, false);
            if (handyBarSlots.Count == 15)
                handyBarSlots.RemoveAt(14);
            handyBarSlots.Insert(0, slot);
            handyBar.Insert(0, slot.VisualElement());
            if (handyBar.childCount > 10)
                handyBar.RemoveAt(handyBar.childCount - 1);
            SaveHandySlots();
        }

        public void RemoveFromHandyPanel(InventorySlotWrapper slot)
        {
            handyBarSlots.Remove(slot);
            slot.VisualElement().RemoveFromHierarchy();
            while (handyBar.childCount < 10 && handyBarSlots.Count >= 10)
                handyBar.Add(handyBarSlots[handyBar.childCount].VisualElement());
            SaveHandySlots();
        }


        private void ToggleInventory()
        {
            var isVisible = inventoryContainer.style.visibility == Visibility.Visible;
            inventoryContainer.style.visibility = isVisible ? Visibility.Hidden : Visibility.Visible;
            handyPanel.style.right = isVisible ? 5 : 362;
            var background = new StyleBackground
            {
                value = Background.FromSprite(isVisible ? openIcon : closeIcon)
            };
            openCloseInvButton.style.backgroundImage = background;
            if (!isVisible)
                OpenAssetsTab();
        }

        private void ShowInventoryLoadingLayer(bool show)
        {
            inventoryLoadingLayer.style.display =
                show ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void LoadFavoriteItems(Action onDone = null, Action onFail = null)
        {
            ShowInventoryLoadingLayer(true);
            StartCoroutine(restClient.GetAllFavoriteItems(new SearchCriteria(), favItems =>
            {
                favoriteItems = favItems;
                onDone?.Invoke();
                ShowInventoryLoadingLayer(false);
            }, () =>
            {
                ShowInventoryLoadingLayer(false);
                onFail?.Invoke();
            }, this));
        }

        private void UpdateUserColorBlocks()
        {
            if (colorBlocksFoldout.childCount == 2)
                colorBlocksFoldout.RemoveAt(1);
            var colorSlotsContainer = CreateUserColorBlocks();
            colorBlocksFoldout.contentContainer.Add(colorSlotsContainer);
            colorBlocksFoldout.contentContainer.style.height = colorSlotsContainer.style.height.value.value + 75;
        }

        private Foldout CreateColorBlocksFoldout()
        {
            var foldout = CreatePackFoldout("Color Blocks");
            var colorBlockCreator = Resources.Load<VisualTreeAsset>("UiDocuments/ColorBlockCreator").CloneTree();
            var colorPickerToggle = colorBlockCreator.Q<Button>();
            colorPickerToggle.style.height = 70;
            colorPickerToggle.clickable.clicked += () => colorSlotPicker.ToggleColorPicker();
            foldout.contentContainer.Add(colorPickerToggle);
            return foldout;
        }

        private VisualElement CreateUserColorBlocks()
        {
            var playerColorBlocks = ColorBlocks.GetPlayerColorBlocks();
            var size = playerColorBlocks.Count;
            var slotsContainer = new VisualElement();
            for (var i = 0; i < size; i++)
            {
                var slot = new ColorBlockInventorySlot();
                ColorUtility.TryParseHtmlString(playerColorBlocks[i], out var color);
                slot.SetSlotInfo(new SlotInfo(ColorBlocks.GetBlockTypeFromColor(color)));
                slot.SetSize(80);
                slot.SetGridPosition(i, 3);
                SetupFavoriteAction(slot);
                slotsContainer.Add(slot.VisualElement());
            }

            slotsContainer.style.height = 90 * (size / 3 + 1);
            return slotsContainer;
        }

        public void DeleteColorBlock(ColorBlockInventorySlot colorBlockInventorySlot)
        {
            ColorBlocks.RemoveBlockColorFromSaving(colorBlockInventorySlot.color);
            UpdateUserColorBlocks();
        }

        private Foldout CreateBlocksPackFoldout(string name, List<BlockType> blocks)
        {
            var foldout = CreatePackFoldout(name);

            var size = blocks.Count;
            if (size <= 0) return foldout;
            for (var i = 0; i < size; i++)
            {
                var slot = new BlockInventorySlot();
                var slotInfo = new SlotInfo(blocks[i]);
                slot.SetSlotInfo(slotInfo);
                slot.SetSize(80);
                slot.SetGridPosition(i, 3);
                SetupFavoriteAction(slot);
                foldout.contentContainer.Add(slot.VisualElement());
            }

            Utils.SetGridContainerSize(foldout.contentContainer, size);
            return foldout;
        }

        private static Foldout CreatePackFoldout(string name)
        {
            var foldout = new Foldout
            {
                text = name
            };
            foldout.SetValueWithoutNotify(true);
            foldout.AddToClassList("utopia-foldout");
            var fs = foldout.style;
            fs.marginRight = fs.marginLeft = fs.marginBottom = fs.marginTop = 5;
            return foldout;
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
            SetAssetsTabContent(scrollView, OpenAssetsTab, "Categories");
        }

        private void OpenAssetsTab()
        {
            breadcrumb.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            tabPane.OpenTab(0);
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
                SetAssetsTabContent(scrollView, OpenAssetsTab, "Categories");
            };
            return container;
        }

        private void SetAssetsTabContent(VisualElement visualElement, Action onBack, string backButtonText = "Back")
        {
            var assetsTabContent = tabPane.GetTabBody().Q<VisualElement>("content");
            var categoriesView = tabPane.GetTabBody().Q<ScrollView>("categories");
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

            ShowInventoryLoadingLayer(true);
            StartCoroutine(restClient.GetAllAssets(searchCriteria, assets =>
            {
                var assetGroups = GroupAssetsByPack(assets);
                foreach (var assetGroup in assetGroups)
                {
                    var foldout = CreatePackFoldout(packs[assetGroup.Key].name);
                    var size = assetGroup.Value.Count;
                    for (var i = 0; i < size; i++)
                    {
                        var slot = new AssetInventorySlot();
                        var slotInfo = new SlotInfo(assetGroup.Value[i]);
                        slot.SetSlotInfo(slotInfo);
                        slot.SetSize(80);
                        slot.SetGridPosition(i, 3);
                        SetupFavoriteAction(slot);
                        foldout.contentContainer.Add(slot.VisualElement());
                    }

                    foldout.contentContainer.style.height = 90 * (size / 3 + 1);
                    scrollView.Add(foldout);
                }

                ShowInventoryLoadingLayer(false);
            }, () => { ShowInventoryLoadingLayer(false); }, this));
            return scrollView;
        }

        private void SetupFavoriteAction(BaseInventorySlot slot)
        {
            var slotInfo = slot.GetSlotInfo();
            var isFavorite = IsUserFavorite(slotInfo, out _);
            slot.ConfigRightAction(isFavorite ? "Remove from favorites" : "Add to favorites",
                isFavorite ? removeFromFavoriteIcon : addToFavoriteIcon,
                () =>
                {
                    var isFavorite = IsUserFavorite(slotInfo, out _);
                    if (isFavorite)
                        RemoveFromFavorites(slot);
                    else
                        AddToFavorites(slot);
                });
            slot.SetRightActionVisible(true);
        }

        private bool IsUserFavorite(SlotInfo slotInfo, out FavoriteItem favoriteItem)
        {
            foreach (var item in favoriteItems)
            {
                if (item.asset != null &&
                    slotInfo.asset != null &&
                    item.asset.id.Value == slotInfo.asset.id.Value)
                {
                    favoriteItem = item;
                    return true;
                }

                if (
                    item.blockId != null &&
                    slotInfo.block != null &&
                    item.blockId.HasValue && item.blockId.Value == slotInfo.block.id)
                {
                    favoriteItem = item;
                    return true;
                }
            }

            favoriteItem = null;
            return false;
        }

        private void SetBodyContent(VisualElement body, VisualElement visualElement, Action onBack,
            string backButtonText = "Back")
        {
            body.Clear();
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

        public void AddToFavorites(BaseInventorySlot slot)
        {
            var slotInfo = slot.GetSlotInfo();
            ShowInventoryLoadingLayer(true);
            var favoriteItem = new FavoriteItem
            {
                asset = slotInfo.asset,
                blockId = slotInfo.block?.id
            };
            StartCoroutine(restClient.CreateFavoriteItem(favoriteItem, item =>
                {
                    ShowInventoryLoadingLayer(false);
                    favoriteItems.Add(item);
                    SetupFavoriteAction(slot);
                },
                () =>
                {
                    ShowInventoryLoadingLayer(false);
                    //TODO a toast?
                }));
        }

        public void RemoveFromFavorites(FavoriteItem favoriteItem, BaseInventorySlot slot, Action onDone = null)
        {
            ShowInventoryLoadingLayer(true);
            StartCoroutine(restClient.DeleteFavoriteItem(favoriteItem.id.Value,
                () =>
                {
                    ShowInventoryLoadingLayer(false);
                    favoriteItems.Remove(favoriteItem);
                    SetupFavoriteAction(slot);
                    onDone?.Invoke();
                    //TODO a toast?
                }, () =>
                {
                    ShowInventoryLoadingLayer(false);
                    //TODO a toast?
                }));
        }

        public void RemoveFromFavorites(BaseInventorySlot slot)
        {
            var slotInfo = slot.GetSlotInfo();
            if (IsUserFavorite(slotInfo, out var favoriteItem))
                RemoveFromFavorites(favoriteItem, slot);
            else
                throw new Exception("Trying to remove from favorites a slot that is not a favorite");
        }

        public void SelectSlot(InventorySlot slot, bool addToHandyPanel = true)
        {
            selectedSlot?.SetSelected(false);
            if (selectedSlot == slot)
            {
                selectedSlot = null;
                selectedSlotChanged.Invoke(null);
                return;
            }

            selectedSlot = slot;
            slot.SetSelected(true);
            var slotInfo = slot.GetSlotInfo();
            selectedSlotChanged.Invoke(slotInfo);
            if (addToHandyPanel)
                AddToHandyPanel(slotInfo);
        }

        private void SaveHandySlots()
        {
            var items = handyBarSlots.Select(slot => SerializableSlotInfo.FromSlotInfo(slot.GetSlotInfo())).ToList();
            PlayerPrefs.SetString(HANDY_SLOTS_KEY, JsonConvert.SerializeObject(items));
        }

        private List<SlotInfo> GetSavedHandySlots()
        {
            return JsonConvert
                .DeserializeObject<List<SerializableSlotInfo>>(PlayerPrefs.GetString(HANDY_SLOTS_KEY, "[]"))
                .Select(serializedSlotInfo => serializedSlotInfo.ToSlotInfo()).ToList();
        }

        public SlotInfo GetSelectedSlot()
        {
            return selectedSlot.GetSlotInfo();
        }

        public VisualElement GetTooltipRoot()
        {
            return root;
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf && inventory.style.visibility == Visibility.Visible;
        }

        public static AssetsInventory INSTANCE => instance;

        public void ReloadTab()
        {
            tabPane.OpenTab(tabPane.GetCurrentTab());
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Nethereum.Model;
using Source;
using Source.Model;
using Source.Service;
using Source.Service.Ethereum;
using Source.Ui;
using Source.Ui.LoadingLayer;
using Source.Ui.Map;
using Source.Ui.Menu;
using Source.Ui.Profile;
using Source.Ui.TabPane;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour, UiProvider
{
    private static Menu instance;
    private VisualElement root;
    private GameManager gameManager;
    private TabPane tabPane;
    private VisualElement rootPane;
    private Button exitButton;
    private Button saveButton;

    void OnEnable()
    {
        instance = this;
        gameManager = GameManager.INSTANCE;
        root = GetComponent<UIDocument>().rootVisualElement;
        rootPane = root.Q<VisualElement>("root");
        var tabConfigs = new List<TabConfiguration>
        {
            new("Settings", () => new Settings(this), UpdateActions),
            new("Map", () => new Map(), UpdateActions),
            new("Help", () => new Help(), UpdateActions),
            new("Profile", () => new UserProfile(null), () =>
            {
                var userProfile = tabPane.GetTabBody().Children().First() as UserProfile;
                var loadingId = LoadingLayer.Show(userProfile);
                ProfileLoader.INSTANCE.load(Settings.WalletId(),
                    profile =>
                    {
                        userProfile.SetProfile(profile);
                        LoadingLayer.Hide(loadingId);
                    },
                    () =>
                    {
                        userProfile.SetProfile(Profile.FAILED_TO_LOAD_PROFILE);
                        LoadingLayer.Hide(loadingId);
                    });
                UpdateActions();
            }),
        };
        tabPane = new TabPane(tabConfigs);
        rootPane.Add(tabPane);
        gameManager.stateChange.AddListener(state =>
        {
            var serviceInitialized = EthereumClientService.INSTANCE.IsInited();
            switch (state)
            {
                case GameManager.State.MENU:
                {
                    gameObject.SetActive(true);
                    tabPane.OpenTab(0);
                    tabPane.SetTabButtonsAreaVisibility(serviceInitialized);
                    break;
                }
                default:
                    gameObject.SetActive(false);
                    tabPane.CloseTabs();
                    break;
            }
        });
    }

    public void UpdateActions()
    {
        if (saveButton == null)
            CreateActions();
        var serviceInitialized = EthereumClientService.INSTANCE.IsInited();
        switch (tabPane.GetCurrentTab())
        {
            case 0:
                saveButton.style.display =
                    !Settings.IsGuest() && serviceInitialized ? DisplayStyle.Flex : DisplayStyle.None;
                saveButton.SetEnabled(WorldService.INSTANCE.HasChange());
                exitButton.style.display = DisplayStyle.Flex;
                break;
            case 1:
                saveButton.style.display = !Settings.IsGuest() ? DisplayStyle.Flex : DisplayStyle.None;
                saveButton.SetEnabled(WorldService.INSTANCE.HasChange());
                exitButton.style.display = DisplayStyle.Flex;
                break;
            case 2:
                saveButton.style.display = DisplayStyle.None;
                exitButton.style.display = DisplayStyle.None;
                break;
            case 4:
                saveButton.style.display = DisplayStyle.None;
                exitButton.style.display = DisplayStyle.None;
                break;
        }
    }

    private void CreateActions()
    {
        exitButton = new Button();
        exitButton.AddToClassList("utopia-button-primary");
        exitButton.AddToClassList("utopia-action-button");
        UiImageUtils.SetBackground(exitButton, Resources.Load<Sprite>("Icons/exit"));
        exitButton.clickable.clicked += () => gameManager.Exit();
        exitButton.tooltip = "Exit";
        exitButton.AddManipulator(new ToolTipManipulator());
        tabPane.AddLeftAction(exitButton);

        saveButton = new Button();
        saveButton.AddToClassList("utopia-button-primary");
        saveButton.AddToClassList("utopia-action-button");
        UiImageUtils.SetBackground(saveButton, Resources.Load<Sprite>("Icons/save"));
        saveButton.clickable.clicked += () => gameManager.Save();
        saveButton.tooltip = "Save my lands";
        saveButton.AddManipulator(new ToolTipManipulator());
        saveButton.style.display = DisplayStyle.None;
        tabPane.AddRightAction(saveButton);
    }

    public VisualElement VisualElement()
    {
        return root;
    }

    public static Menu INSTANCE => instance;
}
using System.Collections.Generic;
using System.Linq;
using Source;
using Source.Model;
using Source.Service.Ethereum;
using Source.Ui;
using Source.Ui.LoadingLayer;
using Source.Ui.Login;
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
    private Button closeButton;

    void OnEnable()
    {
        instance = this;
        gameManager = GameManager.INSTANCE;
        root = GetComponent<UIDocument>().rootVisualElement;
        rootPane = root.Q<VisualElement>("root");
        rootPane.Clear();
        var tabConfigs = new List<TabConfiguration>
        {
            new("Map", () => new Map()),
            new("Help", () => new Help()),
            new("Profile", () => new UserProfile(null), () =>
            {
                var userProfile = tabPane.GetTabBody().Children().First() as UserProfile;
                var loadingId = LoadingLayer.Show(userProfile);
                ProfileLoader.INSTANCE.load(Login.WalletId(),
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
            }),
        };
        tabPane = new TabPane(tabConfigs, false);
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
        CreateActions();
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

        closeButton = new Button();
        closeButton.AddToClassList("utopia-button-primary");
        closeButton.AddToClassList("utopia-action-button");
        UiImageUtils.SetBackground(closeButton, Resources.Load<Sprite>("Icons/close"));
        closeButton.clickable.clicked += () =>
        {
            gameManager.ReturnToGame();
        };
        // closeButton.tooltip = "Close";
        // closeButton.AddManipulator(new ToolTipManipulator(Side.BottomLeft));
        tabPane.AddRightAction(closeButton);
    }

    public VisualElement VisualElement()
    {
        return root;
    }

    public static Menu INSTANCE => instance;
}
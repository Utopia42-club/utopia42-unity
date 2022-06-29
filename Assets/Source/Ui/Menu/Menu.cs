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
            new("Profile", () => new UserProfile(null), (e) =>
            {
                var userProfile = tabPane.GetTabBody().Children().First() as UserProfile;
                var loading = LoadingLayer.Show(userProfile);
                ProfileLoader.INSTANCE.load(AuthService.WalletId(),
                    profile =>
                    {
                        userProfile.SetProfile(profile);
                        loading.Close();
                    },
                    () =>
                    {
                        userProfile.SetProfile(Profile.FAILED_TO_LOAD_PROFILE);
                        loading.Close();
                    });
            }),
        };
        tabPane = new TabPane(tabConfigs, false);
        rootPane.Add(tabPane);
        gameManager.stateChange.AddListener(state =>
        {
            switch (state)
            {
                case GameManager.State.MENU:
                {
                    gameObject.SetActive(true);
                    tabPane.OpenTab(0);
                    break;
                }
                default:
                    gameObject.SetActive(false);
                    tabPane.CloseCurrent();
                    break;
            }
        });
        CreateActions();
    }

    private void CreateActions()
    {
        exitButton = new Button();
        exitButton.AddToClassList("utopia-action-text-button");
        exitButton.AddToClassList("exit-button");
        exitButton.clickable.clicked += () => gameManager.Exit();
        exitButton.text = "Exit";
        tabPane.AddLeftAction(exitButton);

        closeButton = new Button();
        closeButton.AddToClassList("utopia-stroked-button-primary");
        closeButton.AddToClassList("utopia-action-text-button");
        closeButton.text = "Back to game";
        closeButton.style.marginRight = 0;
        closeButton.clickable.clicked += () => gameManager.ReturnToGame();
        tabPane.AddRightAction(closeButton);
    }

    public VisualElement VisualElement()
    {
        return root;
    }

    public static Menu INSTANCE => instance;
}
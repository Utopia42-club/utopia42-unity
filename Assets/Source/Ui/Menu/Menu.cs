using System.Collections.Generic;
using System.Linq;
using Source;
using Source.Model;
using Source.Service.Auth;
using Source.Ui.Dialog;
using Source.Ui.FocusLayer;
using Source.Ui.LoadingLayer;
using Source.Ui.Map;
using Source.Ui.Menu;
using Source.Ui.Profile;
using Source.Ui.TabPane;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
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
        gameManager = GameManager.INSTANCE;
        if (instance == null)
        {
            instance = this;
            gameManager.stateChange.AddListener(OnGameStateChanged);
            OnGameStateChanged(gameManager.GetState());
        }

        if (gameManager.GetState() != GameManager.State.MENU)
            return;
        root = GetComponent<UIDocument>().rootVisualElement;
        rootPane = root.Q<VisualElement>("root");
        rootPane.Clear();
        var tabConfigs = new List<TabConfiguration>
        {
            new("Map", () => new Map()),
            new("Metaverse", () => new MetaverseMenu()),
            new("Help", () => new Help()),
            new("Profile", () => new UserProfile(null), (e) =>
            {
                var userProfile = tabPane.GetTabBody().Children().First() as UserProfile;
                var loading = LoadingLayer.Show(userProfile);
                ProfileLoader.INSTANCE.load(AuthService.Instance.WalletId(),
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
        CreateActions();
        tabPane.OpenTab(0);
    }

    private void OnGameStateChanged(GameManager.State state)
    {
        if (state == GameManager.State.MENU)
        {
            gameObject.SetActive(true);
            tabPane?.SetEnabled(true);
            root.SetEnabled(true);
        }
        else
        {
            gameObject.SetActive(false);
            tabPane?.CloseCurrent();
            root?.SetEnabled(false);
        }
    }

    private void OnDisable()
    {
        rootPane?.Clear();
        tabPane = null;
    }

    private void CreateActions()
    {
        exitButton = new Button();
        exitButton.AddToClassList("utopia-action-text-button");
        exitButton.AddToClassList("exit-button");
        exitButton.clickable.clicked += () =>
        {
            DialogService.INSTANCE.Show(
                new DialogConfig("Confirm exit", new Label("Are you sure you want to exit?"))
                    .WithCancelAction()
                    .WithAction(new DialogAction("Yes", () => gameManager.Exit(), "utopia-button-secondary"))
            );
        };
        exitButton.text = "Exit";
        exitButton.style.marginLeft = 12;
        tabPane.AddLeftAction(exitButton);

        closeButton = new Button();
        closeButton.AddToClassList("utopia-button-primary");
        closeButton.AddToClassList("utopia-action-text-button");
        closeButton.text = "Back to game";
        closeButton.style.marginRight = 0;
        closeButton.clickable.clicked += () => gameManager.ReturnToGame();
        tabPane.AddRightAction(closeButton);
    }

    public static Menu INSTANCE => instance;
}
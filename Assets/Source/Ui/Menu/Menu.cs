using System.Collections.Generic;
using Source;
using Source.Canvas;
using Source.Service;
using Source.Service.Ethereum;
using Source.Ui;
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
    private Settings settings;
    private VisualElement rootPane;
    private Button exitButton;
    private Button saveButton;
    private Button copyLocationButton;

    void OnEnable()
    {
        instance = this;
        gameManager = GameManager.INSTANCE;
        root = GetComponent<UIDocument>().rootVisualElement;
        rootPane = root.Q<VisualElement>("root");
        settings = new Settings(this);
        var tabConfigs = new List<TabConfiguration>
        {
            new("Settings", settings, () => { }),
        };
        tabPane = new TabPane(tabConfigs);
        rootPane.Add(tabPane);

        exitButton = new Button();
        exitButton.AddToClassList("utopia-button");
        UiImageLoader.SetBackground(exitButton, Resources.Load<Sprite>("Icons/shutdown"));
        exitButton.clickable.clicked += () => gameManager.Exit();
        exitButton.tooltip = "Exit";
        exitButton.AddManipulator(new ToolTipManipulator(root));
        tabPane.AddRightAction(exitButton);

        saveButton = new Button();
        saveButton.AddToClassList("utopia-button");
        UiImageLoader.SetBackground(saveButton, Resources.Load<Sprite>("Icons/save"));
        saveButton.clickable.clicked += () => gameManager.Save();
        saveButton.tooltip = "Save my lands";
        saveButton.AddManipulator(new ToolTipManipulator(root));
        saveButton.style.display = DisplayStyle.None;
        tabPane.AddRightAction(saveButton);

        copyLocationButton = new Button();
        copyLocationButton.AddToClassList("utopia-button");
        UiImageLoader.SetBackground(copyLocationButton, Resources.Load<Sprite>("Icons/pin"));
        copyLocationButton.clickable.clicked += () => gameManager.CopyPositionLink();
        copyLocationButton.tooltip = "Copy current position";
        copyLocationButton.AddManipulator(new ToolTipManipulator(root));
        copyLocationButton.style.display = DisplayStyle.None;
        tabPane.AddRightAction(copyLocationButton);

        gameManager.stateChange.AddListener(state =>
        {
            if (state == GameManager.State.SETTINGS)
            {
                var serviceInitialized = EthereumClientService.INSTANCE.IsInited();
                gameObject.SetActive(true);
                tabPane.OpenTab(0);
                tabPane.SetTabButtonsAreaVisibility(serviceInitialized);
                saveButton.style.display = !Settings.IsGuest() ? DisplayStyle.Flex : DisplayStyle.None;
                saveButton.SetEnabled(WorldService.INSTANCE.HasChange());
                copyLocationButton.style.display = serviceInitialized ? DisplayStyle.Flex : DisplayStyle.None;
                root.SetEnabled(true);
            }
            else
            {
                root.SetEnabled(false);
                gameObject.SetActive(false);
            }
        });
    }

    public VisualElement VisualElement()
    {
        return root;
    }

    public static Menu INSTANCE => instance;
}
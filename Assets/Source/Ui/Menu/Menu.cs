using System.Collections.Generic;
using Source;
using Source.Canvas.Map;
using Source.Ui.TabPane;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
    private static Menu instance;

    public MapInputManager mapInputManager;

    private VisualElement root;
    private List<Button> tabs;
    private GameManager _gameManager;
    private Button mapButton;
    private Button settingsButton;
    private Button closeButton;
    private Button sidePanelButton;

    public bool isMouseDown;
    private TabPane tabPane;

    private void Start()
    {
        instance = this;
    }

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        tabPane = root.Q<TabPane>();

        tabs = new List<Button>();
        _gameManager = GameManager.INSTANCE;

        mapButton = root.Q<Button>("map-tab");
        tabs.Add(mapButton);
        mapButton.clicked += () => _gameManager.OpenMap();

        settingsButton = root.Q<Button>("settings-tab");
        tabs.Add(settingsButton);
        settingsButton.clicked += () => _gameManager.OpenSettings();

        closeButton = root.Q<Button>("close-button");
        closeButton.clicked += () => _gameManager.ReturnToGame();

        sidePanelButton = root.Q<Button>("side-panel-button");
        sidePanelButton.clicked += () => mapInputManager.ToggleSidePanel();
        sidePanelButton.visible = _gameManager.GetState() == GameManager.State.MAP;

        _gameManager.stateChange.AddListener(state =>
            sidePanelButton.SetEnabled(sidePanelButton.visible = state == GameManager.State.MAP));
    }

    public void SelectTabByIndex(int index)
    {
        tabs.ForEach(button => button.RemoveFromClassList("selected-tab"));
        tabs[index].AddToClassList("selected-tab");
    }

    public void SetActionsEnabled(bool e)
    {
        closeButton.SetEnabled(e);
        mapButton.SetEnabled(e);
        settingsButton.SetEnabled(e);
        sidePanelButton.SetEnabled(e);
    }

    public static Menu INSTANCE => instance;
}
using System.Collections.Generic;
using src;
using src.Canvas.Map;
using UnityEngine;
using UnityEngine.UIElements;

public class TabMenu : MonoBehaviour
{
    private static TabMenu instance;

    public MapInputManager mapInputManager;

    private VisualElement root;
    private List<Button> tabs;
    private GameManager _gameManager;
    private Button mapButton;
    private Button settingsButton;
    private Button closeButton;
    private Button sidePanelButton;

    private void Start()
    {
        instance = this;
    }

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
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

    public static TabMenu INSTANCE => instance;
}
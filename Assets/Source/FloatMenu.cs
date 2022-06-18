using Source;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FloatMenu : MonoBehaviour
{
    private VisualElement root;
    private GameManager _gameManager;
    private UnityAction<bool> focusListener;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameManager = GameManager.INSTANCE;

        var mapButton = root.Q<Button>("map");
        mapButton.clicked += () => _gameManager.OpenMap();

        var settingsButton = root.Q<Button>("settings");
        settingsButton.clicked += () => _gameManager.OpenSettings();

        var helpButton = root.Q<Button>("help");
        helpButton.clicked += () => _gameManager.OpenHelpDialog();
        
        var locked = MouseLook.INSTANCE.cursorLocked;
        root.focusable = !locked;
        root.SetEnabled(!locked);
        if (focusListener != null)
            MouseLook.INSTANCE.cursorLockedStateChanged.RemoveListener(focusListener);
        focusListener = locked =>
        {
            root.focusable = !locked;
            root.SetEnabled(!locked);
        };
        MouseLook.INSTANCE.cursorLockedStateChanged.AddListener(focusListener);
    }
}
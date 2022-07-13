using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Source
{
    public class FloatMenu : MonoBehaviour
    {
        private GameManager _gameManager;
        private UnityAction<bool> focusListener;
        private VisualElement root;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            _gameManager = GameManager.INSTANCE;

            var menu = root.Q<Button>("menu");
            menu.clicked += () => _gameManager.OpenMenu();

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
}
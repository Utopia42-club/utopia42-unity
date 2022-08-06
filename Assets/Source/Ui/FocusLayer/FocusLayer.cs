using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.FocusLayer
{
    public class FocusLayer : MonoBehaviour
    {
        public static FocusLayer Instance { get; private set; }

        private VisualElement root;

        private void Start()
        {
            Instance = this;
            root = GetComponent<UIDocument>().rootVisualElement;
            root.focusable = true;
            root.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                {
                    MouseLook.INSTANCE.LockCursor();
                    root.Focus();
                }
            });
            GameManager.INSTANCE.stateChange.AddListener(s =>
            {
                if (s == GameManager.State.PLAYING)
                    root.Focus();
            });
        }

        public bool IsFocused()
        {
            return root.focusController.focusedElement == root;
        }
    }
}
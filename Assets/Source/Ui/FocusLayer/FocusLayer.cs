using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.FocusLayer
{
    public class FocusLayer : MonoBehaviour
    {
        public static FocusLayer Instance { get; private set; }

        private VisualElement root;

        private VisualElement fe;

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

        public bool IsTextInputFocused()
        {
            return root.focusController.focusedElement != null &&
                  root.focusController.focusedElement is TextField;
        }
    }
}
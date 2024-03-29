using System;
using Source.Ui.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Source
{
    public class PropertyEditor : MonoBehaviour
    {
        private static PropertyEditor instance;

        private VisualElement root;
        private ScrollView body;
        private Label label;
        private Button cancelAction;
        private Button saveAction;
        private UnityAction<bool> focusListener;
        public int ReferenceObjectID { private set; get; }
        public bool IsActive => root.style.display != DisplayStyle.None; // TODO ?

        private void Start()
        {
            instance = this;
            SetActive(false);
        }

        private void Update()
        {
            var focusable = Player.INSTANCE.FocusedFocusable;
            if (!GameManager.INSTANCE.IsTextInputFocused() && Input.GetKeyDown(KeyCode.E) && IsActive
                && (focusable == null || focusable is ChunkFocusable or MetaFocusable {Focused: false}))
                Hide();
        }

        public void Show()
        {
            SetActive(true);
        }

        public VisualElement Setup(string uxmlPath, string header, Action onSave, int referenceObjectID)
        {
            return Setup(Ui.Utils.Utils.Create(uxmlPath), header, onSave, referenceObjectID);
        }

        public T Setup<T>(T editor, string header, Action onSave, int referenceObjectID)
            where T : VisualElement
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            body = root.Q<ScrollView>("body");
            Scrolls.IncreaseScrollSpeed(body);
            label = root.Q<Label>("label");
            label.text = header;
            body.Clear();
            body.Add(editor);

            cancelAction = root.Q<Button>("cancel");
            cancelAction.clickable.clicked += Hide;
            saveAction = root.Q<Button>("save");
            saveAction.clickable = new Clickable(() => { });
            saveAction.clickable.clicked += onSave;

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
            ReferenceObjectID = referenceObjectID;
            return editor;
        }

        public void Hide()
        {
            SetActive(false);
        }

        private void SetActive(bool active)
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            root.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            root.SetEnabled(active);
        }

        public static PropertyEditor INSTANCE => instance;
    }
}
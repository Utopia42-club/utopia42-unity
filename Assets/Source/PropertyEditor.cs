using System;
using Source;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PropertyEditor : MonoBehaviour
{
    private static PropertyEditor instance;

    private VisualElement root;
    private VisualElement body;
    private Label label;
    private Button cancelAction;
    private Button saveAction;
    private UnityAction<bool> focusListener;

    private void Start()
    {
        instance = this;
        SetActive(false);
    }

    public void Show()
    {
        SetActive(true);
    }

    public VisualElement Setup(string uxmlPath, string header, Action onSave)
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        body = root.Q<VisualElement>("body");
        label = root.Q<Label>("label");
        label.text = header;
        var editor = Resources.Load<VisualTreeAsset>(uxmlPath).CloneTree();
        body.Clear();
        body.Add(editor);

        cancelAction = root.Q<Button>("cancel");
        cancelAction.clickable.clicked += Hide;
        saveAction = root.Q<Button>("save");
        saveAction.clickable = new Clickable(() => {});
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
    }

    public static PropertyEditor INSTANCE => instance;
}
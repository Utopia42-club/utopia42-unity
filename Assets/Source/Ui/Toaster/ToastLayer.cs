using Source.Ui;
using UnityEngine;
using UnityEngine.UIElements;

public class ToastLayer : MonoBehaviour, UiProvider
{
    private static ToastLayer instance;
    private VisualElement root;

    void Start()
    {
        instance = this;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    public VisualElement VisualElement()
    {
        return root;
    }

    public static ToastLayer INSTANCE => instance;
}
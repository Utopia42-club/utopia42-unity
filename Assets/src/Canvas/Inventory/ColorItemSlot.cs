using System;
using src.Canvas;
using UnityEngine;
using UnityEngine.UI;

public class ColorItemSlot : MonoBehaviour
{
    public Action<Color> onColorCreated;

    public FlexibleColorPicker picker;
    public GameObject colorPickerPanel;
    public Image colorBlockImage;
    public Button addColorBlockButton;
    public Button closeButton;
    public Button togglePanelButton;
    private DragAndDropHandler dragAndDropHandler;

    void Start()
    {
        dragAndDropHandler = FindObjectOfType<DragAndDropHandler>();

        addColorBlockButton.onClick.AddListener(() => { onColorCreated?.Invoke(picker.color); });

        closeButton.onClick.AddListener(ToggleColorPicker);

        togglePanelButton.onClick.AddListener(ToggleColorPicker);
    }

    void Update()
    {
        if (picker != null)
        {
            colorBlockImage.color = picker.color;
        }
    }

    private void OnDisable()
    {
        if (colorPickerPanel.activeSelf)
            ToggleColorPicker();
    }

    private void ToggleColorPicker()
    {
        colorPickerPanel.SetActive(!colorPickerPanel.activeSelf);
        dragAndDropHandler.enabled = !colorPickerPanel.activeSelf;
    }

    public void SetPanelSide(ColorPanelSide side)
    {
        var rectTransform = colorPickerPanel.GetComponent<RectTransform>();
        switch (side)
        {
            case ColorPanelSide.Left:
                rectTransform.anchoredPosition3D = new Vector3(-48, 48, 0);
                break;
            case ColorPanelSide.Right:
                rectTransform.anchoredPosition3D = new Vector3(220, 48, 0);
                break;
            case ColorPanelSide.Middle:
                rectTransform.anchoredPosition3D = new Vector3(86, 48, 0);
                break;
        }
    }

    public enum ColorPanelSide
    {
        Left,
        Right,
        Middle,
    }
}
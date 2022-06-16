using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorSlotPicker : MonoBehaviour
{
    [SerializeField] private FlexibleColorPicker picker;
    [SerializeField] private GameObject colorPickerPanel;
    [SerializeField] private Image colorBlockImage;
    [SerializeField] private Button addColorBlockButton;
    [SerializeField] private Button closeButton;

    private Action<Color> onColorCreated;

    void Start()
    {
        addColorBlockButton.onClick.AddListener(() =>
        {
            onColorCreated?.Invoke(picker.color);
            ToggleColorPicker();
        });

        closeButton.onClick.AddListener(ToggleColorPicker);
    }

    void Update()
    {
        if (picker != null)
            colorBlockImage.color = picker.color;
    }

    private void OnDisable()
    {
        if (colorPickerPanel.activeSelf)
            ToggleColorPicker();
    }

    public void SetOnColorCreated(Action<Color> onColorCreated)
    {
        this.onColorCreated = onColorCreated;
    }

    public void ToggleColorPicker()
    {
        colorPickerPanel.SetActive(!colorPickerPanel.activeSelf);
    }

    public bool IsOpen()
    {
        return colorPickerPanel.activeSelf;
    }
}
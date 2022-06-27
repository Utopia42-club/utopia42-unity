using System;
using UnityEngine;
using UnityEngine.UI;

namespace Source.Ui
{
    public class FloatColorPicker : MonoBehaviour
    {
        [SerializeField] private FlexibleColorPicker picker;
        [SerializeField] private GameObject colorPickerPanel;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;

        private Action<Color> onColorCreated;

        void Start()
        {
            submitButton.onClick.AddListener(() =>
            {
                onColorCreated?.Invoke(picker.color);
                ToggleColorPicker();
            });

            closeButton.onClick.AddListener(ToggleColorPicker);
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

        public void SetColor(Color color)
        {
            picker.SetColor(color);
        }
    }
}
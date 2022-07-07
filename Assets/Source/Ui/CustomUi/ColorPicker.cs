using System;
using Source.Canvas;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.CustomUi
{
    public class ColorPicker : UxmlElement
    {
        private readonly VisualElement colorPreview;
        private readonly SliderInt redSlider;
        private readonly SliderInt greenSlider;
        private readonly SliderInt blueSlider;
        private readonly TextField hexField;
        private readonly Button submitButton;
        private Color color;
        private bool updatingUi = false;

        public ColorPicker(Action<Color> onSubmit) : base(typeof(ColorPicker))
        {
            colorPreview = this.Q<VisualElement>("colorPreview");
            redSlider = this.Q<SliderInt>("red");
            greenSlider = this.Q<SliderInt>("green");
            blueSlider = this.Q<SliderInt>("blue");
            hexField = this.Q<TextField>("hexField");
            submitButton = this.Q<Button>("submitButton");
            redSlider.RegisterValueChangedCallback(evt => OnSlidersChanged());
            greenSlider.RegisterValueChangedCallback(evt => OnSlidersChanged());
            blueSlider.RegisterValueChangedCallback(evt => OnSlidersChanged());
            hexField.RegisterValueChangedCallback(evt => OnHexFieldChanged());
            submitButton.clickable.clicked += () => onSubmit.Invoke(color);
        }

        private void OnSlidersChanged()
        {
            if (updatingUi)
                return;
            color = new Color(redSlider.value / 255f, greenSlider.value / 255f, blueSlider.value / 255f);
            hexField.SetValueWithoutNotify(Colors.ConvertToHex(color));
            colorPreview.style.backgroundColor = color;
        }

        private void OnHexFieldChanged()
        {
            if (updatingUi)
                return;
            var c = Colors.ConvertHexToColor(hexField.value);
            if (c == null) return;
            color = c.Value;
            UpdateUi();
        }

        private void UpdateUi()
        {
            updatingUi = true;
            hexField.SetValueWithoutNotify(Colors.ConvertToHex(color));
            redSlider.value = (int) (color.r * 255);
            greenSlider.value = (int) (color.g * 255);
            blueSlider.value = (int) (color.b * 255);
            colorPreview.style.backgroundColor = color;
            updatingUi = false;
        }

        public void SetColor(Color color)
        {
            this.color = color;
            UpdateUi();
        }

        public Color GetColor()
        {
            return color;
        }
    }
}
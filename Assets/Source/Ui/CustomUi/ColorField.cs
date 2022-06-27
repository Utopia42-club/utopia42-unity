using Source.Ui.Popup;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.CustomUi
{
    public class ColorField : UxmlElement
    {
        private readonly VisualElement colorPreview;
        private readonly Label label;
        private readonly Button pickerButton;
        private Color color;

        public ColorField(string labelText) : base("Ui/CustomUi/ColorField")
        {
            colorPreview = this.Q<VisualElement>("colorPreview");
            label = this.Q<Label>("label");
            label.text = labelText;
            pickerButton = this.Q<Button>("pickerButton");
            pickerButton.clickable.clicked += () =>
            {
                var popupId = 0;
                var colorPicker = new ColorPicker(color =>
                {
                    this.color = color;
                    colorPreview.style.backgroundColor = color;
                    PopupService.INSTANCE.Close(popupId);
                });
                colorPicker.SetColor(this.color);
                popupId = PopupService.INSTANCE.Show(new PopupConfig(colorPicker, pickerButton, Side.BottomLeft)
                    .WithWidth(250));
            };
        }

        public Color GetColor()
        {
            return color;
        }

        public void SetColor(Color color)
        {
            this.color = color;
            colorPreview.style.backgroundColor = color;
        }
    }
}
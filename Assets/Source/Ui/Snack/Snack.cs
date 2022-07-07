using UnityEngine.UIElements;

namespace Source.Ui.Snack
{
    public class Snack : UxmlElement
    {
        public Snack(SnackConfig config, int id) : base(typeof(Snack))
        {
            var header = this.Q<VisualElement>("header");
            var content = this.Q<VisualElement>("content");
            var closeButton = this.Q<Button>("closeButton");
            var titleLabel = this.Q<Label>("titleLabel");

            if (config.Title == null && !config.CloseButtonVisible)
                header.style.display = DisplayStyle.None;

            content.Add(config.Content);

            titleLabel.text = config.Title ?? "";
            closeButton.style.display = config.CloseButtonVisible ? DisplayStyle.Flex : DisplayStyle.None;

            closeButton.clickable.clicked += () => SnackService.INSTANCE.Close(id);
        }
    }
}
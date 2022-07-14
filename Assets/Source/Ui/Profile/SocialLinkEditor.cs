using UnityEngine.UIElements;

namespace Source.Ui.Profile
{
    public class SocialLinkEditor : UxmlElement
    {
        private readonly DropdownField typeField;
        private readonly TextField urlField;
        private readonly Button deleteButton;

        public SocialLinkEditor() : base(typeof(SocialLinkEditor))
        {
            typeField = this.Q<DropdownField>("typeField");
            urlField = this.Q<TextField>("urlField");
            deleteButton = this.Q<Button>("deleteButton");
        }

        public void SetValue(Model.Profile.Link link)
        {
            typeField.value = link.media;
            urlField.value = urlField.value;
        }

        public Model.Profile.Link GetValue()
        {
            return new Model.Profile.Link
            {
                media = typeField.value,
                link = urlField.value
            };
        }
    }
}
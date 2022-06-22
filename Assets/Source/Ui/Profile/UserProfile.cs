using Source.Ui.AssetsInventory;
using Source.Ui.Map;
using Source.Ui.Menu;
using Source.Ui.Utils;
using Source.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Profile
{
    public class UserProfile : UxmlElement
    {
        private static readonly Sprite emptyUserIcon = Resources.Load<Sprite>("Icons/empty-user-icon");
        private static readonly Sprite editIcon = Resources.Load<Sprite>("Icons/edit");
        private VisualElement imageElement;
        private Label nameLabel;
        private ScrollView body;

        public UserProfile(Model.Profile profile) : base("Ui/Profile/UserProfile", true)
        {
            SetProfile(profile);
        }

        public void SetProfile(Model.Profile profile)
        {
            if (profile == null)
                return;

            imageElement = this.Q<VisualElement>("profileImage");
            if (profile.imageUrl != null)
            {
                var url = Constants.ApiURL + "/profile/avatar/" + profile.imageUrl;
                GameManager.INSTANCE.StartCoroutine(
                    UiImageUtils.SetBackGroundImageFromUrl(url, emptyUserIcon, imageElement));
            }

            nameLabel = this.Q<Label>("name");
            if (profile.name != null)
                nameLabel.text = profile.name;

            body = this.Q<ScrollView>("body");
            body.Clear();
            if (profile.bio != null)
            {
                var bioLabel = new Label
                {
                    text = profile.bio,
                    style =
                    {
                        flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
                        width = new StyleLength(new Length(100f, LengthUnit.Percent)),
                        whiteSpace = WhiteSpace.Normal,
                    }
                };
                body.Add(bioLabel);
            }

            if (profile.links != null)
            {
                var socialLinks = new VisualElement
                {
                    style =
                    {
                        paddingTop = 10
                    }
                };
                for (var index = 0; index < profile.links.Count; index++)
                {
                    var link = profile.links[index];
                    var socialLink = new SocialLink(link);
                    socialLinks.Add(socialLink);
                    GridUtils.SetChildPosition(socialLink, 150, 40, index, 2);
                }

                GridUtils.SetContainerSize(socialLinks, profile.links.Count, 40, 2);
                body.Add(socialLinks);
            }

            if (profile.walletId.Equals(Settings.WalletId()))
            {
                var editButton = new Button
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 5,
                        left = 158,
                        width = 30,
                        height = 30
                    }
                };
                editButton.AddToClassList("utopia-button-primary");
                UiImageUtils.SetBackground(editButton, editIcon);
                editButton.clickable.clicked += () => GameManager.INSTANCE.EditProfile();
                Add(editButton);
            }
        }
    }
}
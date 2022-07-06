using System.Collections.Generic;
using Source.Canvas;
using Source.Ui.Utils;
using Source.Utils;
using Source.UtopiaException;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Profile
{
    public class UserProfile : UxmlElement
    {
        private static readonly Sprite emptyUserIcon = Resources.Load<Sprite>("Icons/empty-user-icon");
        private VisualElement imageElement;
        private Label nameLabel;
        private ScrollView body;
        private Texture2D loadedProfileImage;

        public UserProfile(Model.Profile profile) : base("Ui/Profile/UserProfile", true)
        {
            SetProfile(profile);
        }

        public void SetProfile(Model.Profile profile)
        {
            if (loadedProfileImage != null)
            {
                Object.Destroy(loadedProfileImage);
                loadedProfileImage = null;
            }

            if (profile == null)
                return;

            imageElement = this.Q<VisualElement>("profileImage");
            if (profile.imageUrl != null)
            {
                var url = Constants.ApiURL + "/profile/image/" + profile.imageUrl;
                GameManager.INSTANCE.StartCoroutine(
                    UiImageUtils.SetBackGroundImageFromUrl(url, emptyUserIcon, false, imageElement, () =>
                    {
                        if (loadedProfileImage != null)
                        {
                            Object.Destroy(loadedProfileImage);
                            loadedProfileImage = null;
                        }

                        var texture = imageElement.style.backgroundImage.value.texture;
                        if (texture == null)
                            throw new IllegalStateException();
                        loadedProfileImage = texture;
                    }));
            }
            else
                UiImageUtils.SetBackground(imageElement, emptyUserIcon, false);

            nameLabel = this.Q<Label>("name");
            if (profile.name != null)
                nameLabel.text = profile.name;

            body = this.Q<ScrollView>("body");
            Scrolls.IncreaseScrollSpeed(body);
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

            if (profile.links == null || profile.links.Count == 0)
            {
                profile.links = new List<Model.Profile.Link>();
                foreach (var media in Model.Profile.Link.medias)
                    profile.links.Add(new Model.Profile.Link
                    {
                        media = media.Key,
                        link = null
                    });
            }

            var socialLinks = this.Q<VisualElement>("socialLinks");
            socialLinks.Clear();
            for (var index = 0; index < profile.links.Count; index++)
            {
                var link = profile.links[index];
                var socialLink = new SocialLink(link);
                socialLinks.Add(socialLink);
            }

            var editButton = this.Q<Button>("userEditButton");
            var designerButton = this.Q<Button>("openAvatarDesigner");
            if (!AuthService.IsGuest() && Equals(profile.walletId, AuthService.WalletId()))
            {
                editButton.clickable = new Clickable(() => { });
                editButton.clickable.clicked += () => BrowserConnector.INSTANCE.EditProfile(() =>
                {
                    ProfileLoader.INSTANCE.InvalidateProfile(profile.walletId);
                    ProfileLoader.INSTANCE.load(profile.walletId, p =>
                    {
                        SetProfile(p);
                        Player.INSTANCE.DoReloadAvatar(p.avatarUrl);
                    }, () =>
                    {
                        //FIXME Show error snack
                    });
                }, () => { });
                designerButton.clickable = new Clickable(() => { });
                designerButton.clickable.clicked += () => Application.OpenURL(Constants.AvatarDesignerURL);
            }
            else
            {
                editButton.style.display = DisplayStyle.None;
                designerButton.style.display = DisplayStyle.None;
            }
        }
    }
}
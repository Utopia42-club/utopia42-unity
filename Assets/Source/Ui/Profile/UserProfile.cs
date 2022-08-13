using Source.Canvas;
using Source.Reactive.Producer;
using Source.Service;
using Source.Service.Auth;
using Source.Ui.LoadingLayer;
using Source.Ui.Utils;
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
        private LoadingController imgLoading;
        private Subscription imgSubscription;

        public UserProfile() : base(typeof(UserProfile), true)
        {
            RegisterCallback<DetachFromPanelEvent>(e => { ClearImageLoadingResources(); });
        }

        private void ClearImageLoadingResources()
        {
            imgLoading?.Close();
            imgLoading = null;
            imgSubscription?.Unsubscribe();
            imgSubscription = null;
        }

        public void SetProfile(string wallet, Model.Profile profile)
        {
            ClearImageLoadingResources();
            if (loadedProfileImage != null)
            {
                Object.Destroy(loadedProfileImage);
                loadedProfileImage = null;
            }

            imageElement = this.Q<VisualElement>("profileImage");
            if (profile?.avatarUrl != null)
            {
                imgLoading = LoadingLayer.LoadingLayer.Show(imageElement);
                var urlObs = ProfileRestClient.INSTANCE.GetProfileImageUrl(profile.avatarUrl);
                imgSubscription = urlObs.Subscribe(url =>
                {
                    ClearImageLoadingResources();
                    if (url == null)
                        UiImageUtils.SetBackground(imageElement, emptyUserIcon, false);
                    else
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
                }, e => ClearImageLoadingResources(), ClearImageLoadingResources);
            }
            else
                UiImageUtils.SetBackground(imageElement, emptyUserIcon, false);

            nameLabel = this.Q<Label>("name");
            nameLabel.text = profile?.name;

            this.Q<Label>("CtizenId").text = profile == null ? "No Citizen Id!" : $"Citizen #{profile.citizenId}";

            body = this.Q<ScrollView>("body");
            Scrolls.IncreaseScrollSpeed(body);
            var bio = body.Q<Label>("Bio");
            bio.text = profile?.bio;
            var props = body.Q("AdditionalProps");
            props.Clear();
            if (profile?.properties != null)
            {
                foreach (var property in profile.properties)
                    props.Add(new PropertyView(property));
            }

            // if (profile?.links == null || profile.links.Count == 0)
            // {
            //     profile.links = new List<Model.Profile.Link>();
            //     foreach (var media in Model.Profile.Link.medias)
            //         profile.links.Add(new Model.Profile.Link
            //         {
            //             media = media.Key,
            //             link = null
            //         });
            // }

            var socialLinks = this.Q<VisualElement>("socialLinks");
            socialLinks.Clear();
            if (profile?.links != null)
            {
                for (var index = 0; index < profile.links.Count; index++)
                {
                    var link = profile.links[index];
                    var socialLink = new SocialLink(link);
                    socialLinks.Add(socialLink);
                }
            }

            var editButton = this.Q<Button>("userEditButton");
            if (wallet != null &&
                !AuthService.Instance.IsGuest() && AuthService.Instance.IsCurrentUser(wallet))
            {
                editButton.clickable = new Clickable(() => { });
                editButton.clickable.clicked += () => BrowserConnector.INSTANCE.OpenDApp(() =>
                {
                    ProfileLoader.INSTANCE.InvalidateProfile(wallet);
                    ProfileLoader.INSTANCE.load(wallet, p =>
                    {
                        SetProfile(wallet, p);
                        Player.INSTANCE.DoReloadAvatar(p.avatarUrl);
                    }, () =>
                    {
                        //FIXME Show error snack
                    });
                }, () => { });
            }
            else
                editButton.style.display = DisplayStyle.None;
        }

        private class PropertyView : VisualElement
        {
            public PropertyView(Model.Profile.Property property)
            {
                AddToClassList("prop-row");
                var key = new Label(property.key);
                key.AddToClassList("prop-key");
                Add(key);
                var value = new Label(property.key);
                value.AddToClassList("prop-value");
                Add(value);
            }
        }
    }
}
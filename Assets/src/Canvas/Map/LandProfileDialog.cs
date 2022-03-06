using System;
using System.Collections.Generic;
using src.Model;
using src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class LandProfileDialog : MonoBehaviour, IPointerClickHandler
    {
        private static LandProfileDialog instance;

        public ActionButton closeButton;

        [Header("Profile")] public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI bioLabel;
        public ImageLoader profileImage;
        public GameObject socialLinks;
        public SocialLink socialLinkPrefab;
        public GameObject editButton;
        private readonly List<GameObject> links = new List<GameObject>();

        [Header("Land")] public TextMeshProUGUI landIdLabel;
        public TextMeshProUGUI landSizeLabel;
        public TMP_InputField landNameField;
        public Button landColorButton;
        public GameObject colorPickerPrefab;
        public GameObject landNftIcon;
        public GameObject colorPickerPlaceHolder;
        public Button transferButton;
        public Button toggleNftButton;
        public Land land;

        private bool isColorPickerOpen;
        private GameObject pickerInstance;

        private GameManager manager;
        private Image landColorButtonImage;
        private FlexibleColorPicker picker;

        private List<Action> onCloseActions = new List<Action>();

        void Start()
        {
            instance = this;
            gameObject.SetActive(false);
            manager = GameManager.INSTANCE;
            manager.stateChange.AddListener((state) =>
            {
                if (gameObject.activeSelf && state != GameManager.State.PROFILE_DIALOG &&
                    state != GameManager.State.MAP)
                    Close();
            });
            editButton.GetComponent<ActionButton>().AddListener(() => manager.EditProfile());
            closeButton.AddListener(Close);
            transferButton.onClick.AddListener(DoTransfer);
            toggleNftButton.onClick.AddListener(DoToggleNft);
            landColorButton.onClick.AddListener(ToggleColorPicker);
            landColorButtonImage = landColorButton.GetComponent<Image>();
        }

        private void Update()
        {
            if (picker != null)
            {
                landColorButtonImage.color = picker.color;
                var newColor = "#" + ColorUtility.ToHtmlStringRGB(picker.color);
                land.properties ??= new LandProperties();
                land.properties.color = newColor;
            }

            land.properties ??= new LandProperties();
            land.properties.name = landNameField.text;
        }

        public void WithOneClose(Action action)
        {
            onCloseActions.Add(action);
        }

        private void ToggleColorPicker()
        {
            if (!Settings.WalletId().Equals(land.owner))
                return;
            if (isColorPickerOpen)
            {
                isColorPickerOpen = false;
                Destroy(pickerInstance);
                picker = null;
                pickerInstance = null;
            }
            else
            {
                pickerInstance = Instantiate(colorPickerPrefab, colorPickerPlaceHolder.transform);
                picker = pickerInstance.GetComponent<FlexibleColorPicker>();
                picker.SetColor(Colors.GetLandColor(land));
                isColorPickerOpen = true;
            }
        }

        private void DoTransfer()
        {
            GameManager.INSTANCE.Transfer(land.id);
        }

        private void DoToggleNft()
        {
            GameManager.INSTANCE.SetNFT(land, !land.isNft);
        }

        public void Close()
        {
            nameLabel.SetText("");
            bioLabel.SetText("");
            profileImage.SetUrl(null);
            if (links.Count > 0)
            {
                foreach (var link in links) DestroyImmediate(link);
                links.Clear();
            }

            if (isColorPickerOpen)
                ToggleColorPicker();

            gameObject.SetActive(false);
            manager.SetProfileDialogState(false);
            onCloseActions.ForEach(action => action());
            onCloseActions.Clear();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isColorPickerOpen)
                ToggleColorPicker();
        }

        public void Open(Land land, Profile profile)
        {
            gameObject.SetActive(true);
            SetLand(land);
            SetProfile(profile);
            manager.SetProfileDialogState(true);
        }

        public void SetProfile(Profile profile)
        {
            if (profile == null)
            {
                nameLabel.SetText("No Profile Found!");
                bioLabel.SetText("");
                profileImage.SetUrl(null);
                return;
            }

            if (profile.name != null)
                nameLabel.SetText(profile.name);
            if (profile.bio != null)
                bioLabel.SetText(profile.bio);
            if (profile.imageUrl != null)
                profileImage.SetUrl(Constants.ApiURL + "/profile/avatar/" + profile.imageUrl);
            if (profile.links != null)
            {
                foreach (var link in profile.links)
                {
                    var socialLink = Instantiate(socialLinkPrefab, socialLinks.transform);
                    socialLink.link = link.link;
                    socialLink.media = link.GetMedia();
                    links.Add(socialLink.gameObject);
                }
            }

            editButton.SetActive(profile.walletId.Equals(Settings.WalletId()));
        }

        public void SetLand(Land land)
        {
            this.land = land;
            landIdLabel.SetText(land.id.ToString());
            var rect = land.ToRect();
            landSizeLabel.SetText((rect.width * rect.height).ToString());
            landNftIcon.SetActive(land.isNft);
            transferButton.gameObject.SetActive(!land.isNft && land.owner.Equals(Settings.WalletId()));
            toggleNftButton.gameObject.SetActive(land.owner.Equals(Settings.WalletId()));

            landColorButtonImage.color = Colors.GetLandColor(land);

            if (toggleNftButton.gameObject.activeSelf)
            {
                toggleNftButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    land.isNft ? "Remove NFT" : "Make NFT";
            }

            landNameField.SetTextWithoutNotify(land.GetName());
            landNameField.interactable = land.owner.Equals(Settings.WalletId());
        }

        public static LandProfileDialog INSTANCE => instance;
    }
}
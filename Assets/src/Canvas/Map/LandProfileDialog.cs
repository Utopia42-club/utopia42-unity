using System.Collections.Generic;
using src.Model;
using src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas.Map
{
    public class LandProfileDialog : MonoBehaviour
    {
        public ActionButton closeButton;
        public Map map;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI bioLabel;
        public ImageLoader profileImage;
        public GameObject socialLinks;
        public SocialLink socialLinkPrefab;
        public GameObject editButton;
        private readonly List<GameObject> links = new List<GameObject>();
        public TextMeshProUGUI landIdLabel;
        public TextMeshProUGUI landSizeLabel;
        public GameObject landNftIcon;
        public Button transferButton;
        public Button toggleNftButton;
        public Land land;
        public Profile profile;

        private GameManager manager;

        void Start()
        {
            manager = GameManager.INSTANCE;
            editButton.GetComponent<ActionButton>().AddListener(() => manager.EditProfile());
            closeButton.AddListener(Close);
            transferButton.onClick.AddListener(DoTransfer);
            toggleNftButton.onClick.AddListener(DoToggleNft);
        }

        private void DoTransfer()
        {
            GameManager.INSTANCE.Transfer(land.id);
        }

        private void DoToggleNft()
        {
            GameManager.INSTANCE.SetNFT(land, !land.isNft);
        }

        private void Close()
        {
            map.CloseLandProfileDialogState();
        }

        public void SetProfile(Profile profile)
        {
            this.profile = profile;
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
            landSizeLabel.SetText(((land.x2 - land.x1) * (land.y2 - land.y1)).ToString());
            landNftIcon.SetActive(land.isNft);
            transferButton.gameObject.SetActive(!land.isNft && land.owner.Equals(Settings.WalletId()));
            toggleNftButton.gameObject.SetActive(land.owner.Equals(Settings.WalletId()));
            if (toggleNftButton.gameObject.activeSelf)
            {
                toggleNftButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    land.isNft ? "Remove NFT" : "Make NFT";
            }
        }

        public void RequestClose()
        {
            nameLabel.SetText("");
            bioLabel.SetText("");
            profileImage.SetUrl(null);
            if (links.Count > 0)
            {
                foreach (var link in links) DestroyImmediate(link);
                links.Clear();
            }
        }
    }
}
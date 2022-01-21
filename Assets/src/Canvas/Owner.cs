using src.Model;
using src.Service;
using src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class Owner : MonoBehaviour
    {
        private static Owner instance;
        public TextMeshProUGUI label;
        public ActionButton openProfileButton;
        [SerializeField] private GameObject view;
        [SerializeField] private ImageLoader profileIcon;

        private GameManager manager;
        private Land prevLand;
        private string prevWallet;
        private Profile currentProfile;
        private Land currentLand;
        private string currentWallet;
        private ProfileLoader profileLoader;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            manager = GameManager.INSTANCE;
            profileLoader = ProfileLoader.INSTANCE;
            openProfileButton.AddListener(() => manager.ShowProfile(currentProfile, null));
        }

        // Update is called once per frame
        void Update()
        {
            if (manager.GetState() == GameManager.State.PLAYING)
            {
                var player = Player.INSTANCE;
                var changed = IsLandChanged(player.transform.position);
                if (changed || !view.activeSelf && currentWallet != null)
                    OnOwnerChanged();

                if (Input.GetButtonDown("Profile") && currentWallet != null &&
                    !profileLoader.IsWalletLoading(currentWallet))
                    manager.ShowProfile(currentProfile, currentLand);
            }
            else
                view.SetActive(false);
        }

        private void OnOwnerChanged()
        {
            if (currentWallet != null)
                LoadProfile();
            else
                SetCurrentProfile(null);
        }

        private void SetCurrentProfile(Profile profile)
        {
            if (profile == null)
            {
                profileIcon.SetUrl(null);
                currentProfile = null;
                view.SetActive(false);
            }
            else
            {
                view.SetActive(true);
                currentProfile = profile;
                profileIcon.SetUrl(profile.imageUrl == null
                    ? null
                    : Constants.ApiURL + "/profile/avatar/" + profile.imageUrl);
                label.SetText(profile.name);
            }
        }

        internal void OnProfileEdited()
        {
            var owner = Settings.WalletId();
            profileLoader.InvalidateProfile(owner);
            if (currentWallet != null && currentWallet.Equals(owner))
                LoadProfile();
        }

        private void LoadProfile()
        {
            view.SetActive(false);
            SetCurrentProfile(Profile.LOADING_PROFILE);
            profileLoader.load(currentWallet, profile =>
            {
                Debug.Log("loaded profile.");
                SetCurrentProfile(profile);
                if (profile != null)
                {
                    HorizontalLayoutGroup layout = view.GetComponentInChildren<HorizontalLayoutGroup>();
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) layout.transform);
                }
            }, () =>
            {
                Debug.Log("Failed to load.");
                SetCurrentProfile(Profile.FAILED_TO_LOAD_PROFILE);
            });
        }

        private bool IsLandChanged(Vector3 position)
        {
            if (currentLand != null && currentLand.Contains(ref position))
                return false;
            if (prevLand != null && prevLand.Contains(ref position))
            {
                prevLand = currentLand;
                prevWallet = currentWallet;
                currentLand = prevLand;
                currentWallet = prevWallet;
                return true;
            }

            var land = VoxelService.INSTANCE.GetLandByPosition(position);
            if (land == null && currentLand == null) return false;

            prevLand = currentLand;
            prevWallet = currentWallet;
            currentLand = land;
            currentWallet = land?.owner;
            return true;
        }


        public static Owner INSTANCE
        {
            get { return instance; }
        }
    }
}
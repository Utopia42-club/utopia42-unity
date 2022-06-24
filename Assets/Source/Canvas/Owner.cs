using System.Collections.Generic;
using Source.Model;
using Source.Service;
using Source.Ui.Menu;
using Source.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Source.Canvas
{
    public class Owner : MonoBehaviour
    {
        private static Owner instance;
        public TextMeshProUGUI label;
        public ActionButton openProfileButton;
        [SerializeField] private GameObject view;
        [SerializeField] private ImageLoader profileIcon;

        private SnackItem snackItem; // TODO ?

        private GameManager manager;
        private Land prevLand;
        private string prevWallet;
        private Profile currentProfile;
        private Land currentLand;
        private string currentWallet;
        private ProfileLoader profileLoader;

        public readonly UnityEvent<object> currentLandChanged = new UnityEvent<object>();

        void Start()
        {
            instance = this;
            manager = GameManager.INSTANCE;
            profileLoader = ProfileLoader.INSTANCE;
            openProfileButton.AddListener(() => manager.ShowProfile(currentProfile, null));
        }

        void Update()
        {
            if (manager.GetState() == GameManager.State.PLAYING)
            {
                var player = Player.INSTANCE;
                var changed = IsLandChanged(player.GetPosition());
                if (changed)
                    currentLandChanged.Invoke(currentLand);
                if (changed || !view.activeSelf && currentWallet != null)
                    OnOwnerChanged();

                if (Input.GetButtonDown("Profile")
                    && currentWallet != null
                    && !profileLoader.IsWalletLoading(currentWallet)
                    && !manager.IsUiEngaged())
                    manager.ShowProfile(currentProfile, currentLand);
            }
            else
            {
                // view.SetActive(false);
                RemoveShortcutsSnack();
            }
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
                // view.SetActive(false);
                RemoveShortcutsSnack();
            }
            else
            {
                // view.SetActive(true);
                ShowShortcutsSnack();
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
            // view.SetActive(false);
            RemoveShortcutsSnack();
            SetCurrentProfile(Profile.LOADING_PROFILE);
            profileLoader.load(currentWallet, profile =>
            {
                SetCurrentProfile(profile);
                if (profile != null)
                {
                    HorizontalLayoutGroup layout = view.GetComponentInChildren<HorizontalLayoutGroup>();
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) layout.transform);
                }
            }, () => { SetCurrentProfile(Profile.FAILED_TO_LOAD_PROFILE); });
        }

        private bool IsLandChanged(Vector3 position)
        {
            if (currentLand != null && currentLand.Contains(position))
                return false;
            if (prevLand != null && prevLand.Contains(position))
            {
                prevLand = currentLand;
                prevWallet = currentWallet;
                currentLand = prevLand;
                currentWallet = prevWallet;
                return true;
            }

            var land = WorldService.INSTANCE.GetLandForPosition(position);
            if (land == null && currentLand == null) return false;

            prevLand = currentLand;
            prevWallet = currentWallet;
            currentLand = land;
            currentWallet = land?.owner;
            return true;
        }
        
        private void ShowShortcutsSnack()
        {
            snackItem?.Remove();
            snackItem = Snack.INSTANCE.ShowLines(new List<string>
            {
                "esc : unlock the cursor",
                "ctrl+B : switch between first and third person view"
            }, () => { });
        }

        private void RemoveShortcutsSnack()
        {
            if (snackItem == null) return;
            snackItem.Remove();
            snackItem = null;
        }

        public static Owner INSTANCE
        {
            get { return instance; }
        }
    }
}
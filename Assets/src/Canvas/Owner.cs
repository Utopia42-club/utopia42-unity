using System;
using System.Collections.Generic;
using src.Model;
using src.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.Canvas
{
    public class Owner : MonoBehaviour
    {
        private static Owner instance;
        private readonly static Profile NO_PROFILE = new Profile();
        private Dictionary<string, Profile> profileCache = new Dictionary<string, Profile>();
        public TextMeshProUGUI label;
        public ActionButton openProfileButton;
        [SerializeField]
        private GameObject view;
        [SerializeField]
        private ImageLoader profileIcon;
        private GameManager manager;
        private Land prevLand;
        private string prevWallet;
        private Profile currentProfile;
        private Land currentLand;
        private string currentWallet;

        // Start is called before the first frame update
        void Start()
        {
            instance = this;
            manager = GameManager.INSTANCE;
            openProfileButton.AddListener(() => manager.ShowProfile(currentProfile));
        }

        // Update is called once per frame
        void Update()
        {
            if (manager.GetState() == GameManager.State.PLAYING)
            {
                var player = Player.INSTANCE;
                var changed = IsLandChanged(player.transform.position);
                if (changed || (!view.activeSelf && currentWallet != null))
                    OnOwnerChanged();
            }
            else
            {
                view.SetActive(false);
            }
            if (manager.GetState() == GameManager.State.PLAYING || manager.GetState() == GameManager.State.PROFILE)
            {
                if (Input.GetButtonDown("Profile"))
                {
                    var state = manager.GetState();
                    if (state == GameManager.State.PROFILE)
                        manager.ReturnToGame();
                    else if (state == GameManager.State.PLAYING && currentWallet != null)
                        manager.ShowProfile(currentProfile);
                }
            }
        }

        private void SetCurrentProfile(Profile profile)
        {
            if (profile == NO_PROFILE || profile == null)
            {
                profileIcon.SetUrl(null);
                currentProfile = null;
                view.SetActive(false);
            }
            else
            {
                view.SetActive(true);
                currentProfile = profile;
                profileIcon.SetUrl(RestClient.SERVER_URL + "profile/avatar/" + profile.imageUrl);
                label.SetText(profile.name);
            }
        }

        private void OnOwnerChanged()
        {
            if (currentWallet != null)
            {
                Profile p;
                if (profileCache.TryGetValue(currentWallet, out p))
                    SetCurrentProfile(p);
                else LoadProfile();
            }
            else
                SetCurrentProfile(null);
        }

        private void LoadProfile()
        {
            view.SetActive(false);
            var wallet = currentWallet;
            StartCoroutine(RestClient.INSATANCE.GetProfile(wallet,
                (profile) =>
                {
                    profileCache[wallet] = profile == null ? NO_PROFILE : profile;
                    if (!wallet.Equals(currentWallet)) return;
                    view.SetActive(profile != null);
                    if (profile != null)
                    {
                        SetCurrentProfile(profile);
                        HorizontalLayoutGroup layout = view.GetComponentInChildren<HorizontalLayoutGroup>();
                        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
                    }
                }, () => { }));
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

            var ownerLands = VoxelService.INSTANCE.GetOwnersLands();
            if (ownerLands != null)
            {
                foreach (var landPair in ownerLands)
                {
                    foreach (var land in landPair.Value)
                    {
                        if (land.Contains(ref position))
                        {
                            prevLand = currentLand;
                            prevWallet = currentWallet;
                            currentLand = land;
                            currentWallet = landPair.Key;
                            return true;
                        }
                    }
                }
            }
            if (currentLand == null) return false;
            currentLand = null;
            currentWallet = null;
            return true;
        }


        public static Owner INSTANCE
        {
            get
            {
                return instance;
            }
        }

        internal void UserProfile(Action<Profile> consumer, Action failed)
        {
            var owner = Settings.WalletId();
            if (owner == null) return;
            Profile profile;
            if (profileCache.TryGetValue(owner, out profile))
                consumer(profile == NO_PROFILE ? null : profile);
            else
            {
                StartCoroutine(RestClient.INSATANCE.GetProfile(owner,
                    (profile) =>
                    {
                        profileCache[owner] = profile == null ? NO_PROFILE : profile;
                        consumer(profile);
                    }, failed));
            }
        }

        internal void OnProfileEdited()
        {
            var owner = Settings.WalletId();
            if (profileCache.ContainsKey(owner))
                profileCache.Remove(owner);
            if (currentWallet != null && currentWallet.Equals(owner))
                LoadProfile();
        }
    }
}

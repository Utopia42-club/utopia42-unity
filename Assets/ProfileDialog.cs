using System.Collections.Generic;
using src;
using src.Canvas;
using src.Model;
using src.Service;
using TMPro;
using UnityEngine;

public class ProfileDialog : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI bioLabel;
    public ImageLoader profileImage;
    public GameObject socialLinks;
    public SocialLink socialLinkPrefab;
    public ActionButton closeButton;
    public GameObject editButton;
    private readonly List<GameObject> links = new List<GameObject>();
    private GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.INSTANCE;
        closeButton.AddListener(() => manager.ReturnToGame());
        editButton.GetComponent<ActionButton>().AddListener(() => manager.EditProfile());
        manager.stateChange.AddListener(s =>
        {
            if (s != GameManager.State.PROFILE)
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
        });
    }

    // Update is called once per frame
    public void SetProfile(Profile profile)
    {
        if (profile == null)
        {
            nameLabel.SetText("No Profile Found!");
            bioLabel.SetText("You can press edit icon to create your profile.");
            profileImage.SetUrl(null);
            return;
        }
        if (profile.name != null)
            nameLabel.SetText(profile.name);
        if (profile.bio != null)
            bioLabel.SetText(profile.bio);
        if (profile.imageUrl != null)
            profileImage.SetUrl(RestClient.SERVER_URL + "profile/avatar/" + profile.imageUrl);
        if (profile.links != null)
        {
            foreach (var link in profile.links)
            {
                SocialLink socialLink = Instantiate(socialLinkPrefab, socialLinks.transform);
                socialLink.link = link.link;
                socialLink.media = link.GetMedia();
                links.Add(socialLink.gameObject);
            }
        }
        editButton.SetActive(profile.walletId.Equals(Settings.WalletId()));
    }

    public static ProfileDialog INSTANCE
    {
        get
        {
            return GameObject.Find("ProfileDialog").GetComponent<ProfileDialog>();
        }
    }

}

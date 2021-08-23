using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfileDialog : MonoBehaviour
{
    public Owner owner;

    public TextMeshProUGUI nameLabel;

    public TextMeshProUGUI bioLabel;

    public ImageLoader profileImage;

    public GameObject socialLinks;

    public SocialLink socialLinkPrefab;

    public GameObject editButton;

    // Start is called before the first frame update
    void Start()
    {
        var profile = owner.GetOwner();
        nameLabel.SetText(profile.name);
        bioLabel.SetText(profile.bio);
        profileImage.SetUrl(RestClient.SERVER_URL + "profile/avatar/" + profile.imageUrl);
        foreach (var link in profile.links)
        {
            SocialLink socialLink = Instantiate(socialLinkPrefab, socialLinks.transform);
            socialLink.link = link.link;
            socialLink.media = link.GetMedia();
        }
        editButton.SetActive(profile.walletId.Equals(Settings.WalletId()));
    }

    // Update is called once per frame
    void Update()
    {

    }
}

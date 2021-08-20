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

    // Start is called before the first frame update
    void Start()
    {
        var profile = owner.getOwner();
        nameLabel.SetText(profile.name);
        bioLabel.SetText(profile.bio);
        profileImage.url = profile.imageUrl;
        foreach (var link in profile.links)
        {
            SocialLink socialLink = Instantiate(socialLinkPrefab, socialLinks.transform);
            socialLink.link = link.link;
            socialLink.media = link.media;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

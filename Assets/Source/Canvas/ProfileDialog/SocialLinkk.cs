using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Source.Model.Profile;

public class SocialLinkk : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    private Image icon;

    public string link;
    public Link.Media media;

    public List<Sprite> mediaIcons;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        icon = GetComponentInChildren<Image>();

        textMeshPro.SetText("<link=" + link + "><u>" + media.GetName() + "</u></link>");
        
        icon.sprite = mediaIcons[media.GetIndex()];
    }
}

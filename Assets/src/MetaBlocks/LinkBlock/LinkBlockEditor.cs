using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LinkBlockEditor : MonoBehaviour
{
    public static readonly string PREFAB = "MetaBlocks/LinkBlockEditor";

    public TMP_Dropdown type;
    public InputField url;
    public InputField x;
    public InputField y;
    public InputField z;
    public GameObject gameLinkEditor;
    public GameObject webLinkEditor;


    private void Update()
    {
        webLinkEditor.SetActive(type.value == 0);
        gameLinkEditor.SetActive(type.value == 1);
    }


    public LinkBlockProperties.FaceProps GetValue()
    {
        int value = type.value;

        if (value == 0)
        {
            if (HasValue(url))
            {
                var props = new LinkBlockProperties.FaceProps();
                props.url = url.text;
                props.type = 0;
                return props;
            }
        }
        else
        {
            if (HasValue(x) && HasValue(y) && HasValue(z))
            {
                var props = new LinkBlockProperties.FaceProps();
                props.type = 1;
                props.x = int.Parse(x.text);
                props.y = int.Parse(y.text);
                props.z = int.Parse(z.text);
                return props;
            }
        }
        return null;
    }

    public void SetValue(LinkBlockProperties.FaceProps value)
    {
        type.value = value == null ? 1 : value.type;
        url.text = value == null ? "" : value.url;
        x.text = value == null ? "" : value.x.ToString();
        y.text = value == null ? "" : value.y.ToString();
        z.text = value == null ? "" : value.z.ToString();
    }

    private bool HasValue(InputField f)
    {
        return f.text != null && f.text.Length > 0;
    }
}


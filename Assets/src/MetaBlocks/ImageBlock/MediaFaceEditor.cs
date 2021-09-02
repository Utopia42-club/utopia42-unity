using UnityEngine;
using UnityEngine.UI;

public class MediaFaceEditor : MonoBehaviour
{
    [SerializeField]
    public InputField url;
    [SerializeField]
    public InputField width;
    [SerializeField]
    public InputField height;

    public MediaBlockProperties.FaceProps GetValue()
    {
        if (HasValue(url) && HasValue(width) && HasValue(height))
        {
            var props = new MediaBlockProperties.FaceProps();
            props.url = url.text;
            props.width = int.Parse(width.text);
            props.height = int.Parse(height.text);
            return props;
        }
        return null;
    }

    public void SetValue(MediaBlockProperties.FaceProps value)
    {
        url.text = value == null ? "" : value.url;
        width.text = value == null ? "" : value.width.ToString();
        height.text = value == null ? "" : value.height.ToString();
    }

    private bool HasValue(InputField f)
    {
        return f.text != null && f.text.Length > 0;
    }
}
using UnityEngine;

public class MediaBlockEditor : MonoBehaviour
{
    public static readonly string PREFAB = "MetaBlocks/MediaBlockEditor";
    [SerializeField]
    private MediaFaceEditor top;
    [SerializeField]
    private MediaFaceEditor bottom;
    [SerializeField]
    private MediaFaceEditor left;
    [SerializeField]
    private MediaFaceEditor right;
    [SerializeField]
    private MediaFaceEditor front;
    [SerializeField]
    private MediaFaceEditor back;

    public void SetValue(MediaBlockProperties props)
    {
        top.SetValue(props == null ? null : props.top);
        bottom.SetValue(props == null ? null : props.bottom);
        left.SetValue(props == null ? null : props.left);
        right.SetValue(props == null ? null : props.right);
        front.SetValue(props == null ? null : props.front);
        back.SetValue(props == null ? null : props.back);
    }

    public MediaBlockProperties GetValue()
    {
        var props = new MediaBlockProperties();
        props.top = top.GetValue();
        props.bottom = bottom.GetValue();
        props.left = left.GetValue();
        props.right = right.GetValue();
        props.front = front.GetValue();
        props.back = back.GetValue();
        return props;
    }
}


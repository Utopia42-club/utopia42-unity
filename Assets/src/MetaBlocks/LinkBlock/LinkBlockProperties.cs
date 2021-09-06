[System.Serializable]
public class LinkBlockProperties
{
    public FaceProps faceProps;

    public LinkBlockProperties()
    {

    }

    public LinkBlockProperties(LinkBlockProperties obj)
    {
        if (obj != null)
        {
            faceProps = obj.faceProps;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        var prop = obj as LinkBlockProperties;
        return Equals(faceProps, prop.faceProps);
    }

    public FaceProps GetFaceProps()
    {
        return faceProps;
    }

    public void SetFaceProps(FaceProps props)
    {
        faceProps = props;
    }

    public bool IsEmpty()
    {
        return faceProps == null;
    }

    [System.Serializable]
    public class FaceProps
    {
        public int type;
        public string url;
        public int x;
        public int y;
        public int z;

        public override bool Equals(object obj)
        {
            return obj is FaceProps props &&
                   type == props.type &&
                   url == props.url &&
                   x == props.x &&
                   y == props.y &&
                   z == props.z;
        }
    }
}
